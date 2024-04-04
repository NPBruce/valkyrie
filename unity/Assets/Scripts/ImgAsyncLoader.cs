using Assets.Scripts.UI.Screens;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts
{
    internal class ImgAsyncLoader
    {
        // URL and UI element
        private Dictionary<string, UIElement> images_list = null;
        // URL and Texture
        private Dictionary<string, Texture2D> texture_list = null;

        Texture2D default_quest_picture = null;

        // Father class
        IContentImageDrawer contentImageDrawer = null;

        public ImgAsyncLoader(IContentImageDrawer contentImageDrawer)
        {
            this.contentImageDrawer = contentImageDrawer;
            images_list = new Dictionary<string, UIElement>();
            texture_list = new Dictionary<string, Texture2D>();
            default_quest_picture = Resources.Load("sprites/scenario_list/default_quest_picture") as Texture2D;
        }

        public void Add(string url, UIElement uie)
        {
            if(!images_list.ContainsKey(url))
            {
                images_list.Add(url, uie);
            }
            else
            {
                ValkyrieDebug.Log($"Duplicate image found: {url}");
            }
            
        }

        public void Clear()
        {
            images_list.Clear();
            // do not clear Texture, we don't want to download pictures again
        }

        public void StartDownloadASync()
        {
            if (images_list.Count > 0)
            {
                foreach (KeyValuePair<string, UIElement> kv in images_list)
                {
                        HTTPManager.GetImage(kv.Key, ImageDownloaded_callback);
                }
            }
        }

        /// <summary>
        /// Parse the downloaded remote manifest and start download of individual quest files
        /// </summary>
        public void ImageDownloaded_callback(Texture2D texture, bool error, Uri uri)
        {
            if (error)
            {
                Debug.Log("Error downloading picture : " + uri.ToString());

                // Display default picture
                if (images_list.ContainsKey(uri.ToString())) // this can be empty if we display another screen while pictures are downloading
                    contentImageDrawer.DrawPicture(null, images_list[uri.ToString()]);
            }
            else
            {
                // we might have started two downloads of the same picture (changing sort options before end of download)
                if (!texture_list.ContainsKey(uri.ToString()))
                {
                    // save texture
                    texture_list.Add(uri.ToString(), texture);

                    // Display pictures
                    if (images_list.ContainsKey(uri.ToString())) // this can be empty if we display another screen while pictures are downloading
                        contentImageDrawer.DrawPicture(GetTexture(uri.ToString()), images_list[uri.ToString()]);
                }
            }
        }

        public bool IsImageAvailable(string package_url)
        {
            return texture_list.ContainsKey(package_url);
        }

        public Texture2D GetTexture(string package_url)
        {
            if (package_url == null)
                return default_quest_picture;
            else
                return texture_list[package_url];
        }
    }
}
