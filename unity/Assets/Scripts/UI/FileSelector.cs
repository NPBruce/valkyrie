using UnityEngine;
using System;

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
                // Desktop implementations using native file dialogs
                string path = NativeFileDialog.OpenFilePanel("Select Import File (ZIP)", "", "zip");
                if (!string.IsNullOrEmpty(path))
                {
                    onFileSelected?.Invoke(path);
                }
            }
        }
    }
}
