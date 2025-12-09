using UnityEngine;
using System;
using SFB;

namespace Assets.Scripts.UI
{
    public static class FileSelector
    {
        /// <summary>
        /// Opens a file picker to select a ZIP or OBB file.
        /// Handles platform differences between Android and Desktop.
        /// </summary>
        /// <param name="onFileSelected">Callback invoked with the selected file path.</param>
        public static void PickZipFile(Action<string> onFileSelected)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                NativeFilePicker.PickFile(delegate (string path)
                {
                    if (string.IsNullOrEmpty(path)) return;
                    onFileSelected?.Invoke(path);
                }, NativeFilePicker.ConvertExtensionToFileType("zip"));
            }
            else
            {
                // Desktop implementations using StandaloneFileBrowser
                string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Import File (Zip/Obb)", "", "", false);
                if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
                {
                    onFileSelected?.Invoke(paths[0]);
                }
            }
        }
    }
}
