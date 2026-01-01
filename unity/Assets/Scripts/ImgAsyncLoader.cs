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

        // Lazy Loading
        UIElementScrollVertical currentScrollArea = null;
        private HashSet<string> requested_urls = new HashSet<string>();

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
            if (currentScrollArea != null)
            {
                currentScrollArea.RemoveScrollListener(Scroll_callback);
                currentScrollArea = null;
            }
            requested_urls.Clear();
            images_list.Clear();
            // do not clear Texture, we don't want to download pictures again
        }

        public void StartDownloadASync(UIElementScrollVertical scrollArea = null)
        {
            if (currentScrollArea != null)
            {
                currentScrollArea.RemoveScrollListener(Scroll_callback);
            }
            currentScrollArea = scrollArea;

            if (currentScrollArea != null)
            {
                currentScrollArea.AddScrollListener(Scroll_callback);
            }

            CheckVisibleImages();
        }

        private void Scroll_callback(Vector2 scroll_pos)
        {
            CheckVisibleImages();
        }

        private void CheckVisibleImages()
        {
            if (images_list.Count == 0) return;

            // Clone keys to avoid modification issues if we were removing (though we are not removing here)
            // But we iterate images_list
            foreach (KeyValuePair<string, UIElement> kv in images_list)
            {
                if (requested_urls.Contains(kv.Key)) continue;

                if (IsVisible(kv.Value))
                {
                    requested_urls.Add(kv.Key);
                    HTTPManager.GetImage(kv.Key, ImageDownloaded_callback);
                }
            }
        }

        private bool IsVisible(UIElement element)
        {
            if (currentScrollArea == null) return true;
            if (element == null || element.ObjectDestroyed()) return false;

            return CheckOverlap(element.GetRectTransform(), currentScrollArea.GetRectTransform());
        }

        private bool CheckOverlap(RectTransform rt1, RectTransform rt2)
        {
            Vector3[] corners1 = new Vector3[4];
            rt1.GetWorldCorners(corners1);

            Vector3[] corners2 = new Vector3[4];
            rt2.GetWorldCorners(corners2);

            // Simple AABB check on world corners
            float xMin1 = Mathf.Min(corners1[0].x, corners1[2].x);
            float xMax1 = Mathf.Max(corners1[0].x, corners1[2].x);
            float yMin1 = Mathf.Min(corners1[0].y, corners1[2].y);
            float yMax1 = Mathf.Max(corners1[0].y, corners1[2].y);

            float xMin2 = Mathf.Min(corners2[0].x, corners2[2].x);
            float xMax2 = Mathf.Max(corners2[0].x, corners2[2].x);
            float yMin2 = Mathf.Min(corners2[0].y, corners2[2].y);
            float yMax2 = Mathf.Max(corners2[0].y, corners2[2].y);

            if (xMax1 < xMin2 || xMin1 > xMax2) return false;
            if (yMax1 < yMin2 || yMin1 > yMax2) return false;

            return true;
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
                {
                    if(!images_list[uri.ToString()].ObjectDestroyed())
                        contentImageDrawer.DrawPicture(null, images_list[uri.ToString()]);
                }
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
                    {
                         if(!images_list[uri.ToString()].ObjectDestroyed())
                            contentImageDrawer.DrawPicture(GetTexture(uri.ToString()), images_list[uri.ToString()]);
                    }
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
