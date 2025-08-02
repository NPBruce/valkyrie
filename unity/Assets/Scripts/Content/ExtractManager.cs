using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ValkyrieTools;

namespace Assets.Scripts.Content
{
    internal class ExtractManager
    {
        /// <summary>
        /// Fully extract one single package, before starting a quest, and save package filename
        /// </summary>
        /// <param name="path">path of the file to extract</param>
        public static string ExtractSinglePackageFull(string path)
        {
            // Extract into temp
            string tempValkyriePath = ContentData.TempValkyriePath;
            mkDir(tempValkyriePath);

            string extractedPath = Path.Combine(tempValkyriePath, Path.GetFileName(path));
            ZipManager.Extract(extractedPath, path, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_FULL);
            return extractedPath;
        }

        /// <summary>
        /// Partial extract of a single package, before listing the savegames
        /// Only the quest.ini and translations needs to be extracted to validate quest and get its name
        /// </summary>
        /// <param name="path">path of the file to extract</param>
        public static void ExtractSinglePackagePartial(string path)
        {
            // Extract into temp
            string extractedPath = Path.Combine(ContentData.TempValkyriePath, Path.GetFileName(path));
            ZipManager.Extract(extractedPath, path, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_INI_TXT_PIC);
        }

        /// <summary>
        /// Partial extract of all package in a directory, to list quests,  and save package filename
        /// </summary>
        /// <param name="path">path of the directory containing .valkyrie package</param>
        public static void ExtractPackages(string path)
        {
            // Find all packages at path
            string[] archives = Directory.GetFiles(path, ValkyrieConstants.ContentPackDownloadContainerExtensionAllFileReference, SearchOption.AllDirectories);

            // Extract all packages
            foreach (string f in archives)
            {
                string extractedPath = Path.Combine(ContentData.TempValkyriePath, Path.GetFileName(f));
                ZipManager.Extract(extractedPath, f, ZipManager.Extract_mode.ZIPMANAGER_EXTRACT_INI_TXT_PIC);
            }
        }

        // Attempt to create a directory
        public static void mkDir(string p)
        {
            if (!Directory.Exists(p))
            {
                try
                {
                    Directory.CreateDirectory(p);
                }
                catch (System.Exception)
                {
                    ValkyrieDebug.Log("Error: Unable to create directory: " + p);
                    Application.Quit();
                }
            }
        }

        // Return a list of directories at a path (recursive)
        public static List<string> DirList(string path)
        {
            return DirList(path, new List<string>());
        }

        // Add to list of directories at a path (recursive)
        public static List<string> DirList(string path, List<string> l)
        {
            List<string> list = new List<string>(l);

            foreach (string s in Directory.GetDirectories(path))
            {
                list = DirList(s, list);
                list.Add(s);
            }

            return list;
        }

        public static void CleanTemp()
        {
            // Nothing to do if no temporary files
            string tempValkyriePath = ContentData.TempValkyriePath;
            if (!Directory.Exists(tempValkyriePath))
            {
                return;
            }

            try
            {
                Directory.Delete(tempValkyriePath, true);
            }
            catch
            {
                ValkyrieDebug.Log("Warning: Unable to remove temporary files.");
            }
        }
    }
}
