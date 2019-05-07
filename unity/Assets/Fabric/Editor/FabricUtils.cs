namespace Fabric.Internal.Editor
{
	using UnityEngine;
	using System;
	using System.IO;

	public static class Utils
	{	
		private static readonly string logPrefix = "[Fabric] ";

		public static void Log (string format, params object[] list)
		{
			Debug.Log (logPrefix + string.Format (format, list));
		}

		public static void Warn (string format, params object[] list)
		{
			Debug.LogWarning (logPrefix + string.Format (format, list));
		}

		public static void Error (string format, params object[] list)
		{
			throw new System.Exception (logPrefix + string.Format (format, list));
		}

		public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}
			
			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string temppath = Update.FileUtils.NormalizePathForPlatform (Path.Combine(destDirName, file.Name));
				file.CopyTo(temppath, false);
			}
			
			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs)
			{
				DirectoryInfo[] dirs = dir.GetDirectories();
				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Update.FileUtils.NormalizePathForPlatform (Path.Combine(destDirName, subdir.Name));
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}
	}

}
