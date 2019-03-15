namespace Fabric.Internal.Editor.Update
{
	using System;
	using System.Collections.Generic;
	using System.Collections;
	using System.IO;
	using UnityEditor;
	using UnityEngine;

	// Cleans up the file tree based on the passed in manifest. The manifest contains all
	// files that should be kept, and all directories that should be ignored when doing
	// cleanup.
	//
	// Given a path to an existing Manifest file, custodian builds two lookup tables;
	//   keep   - contains all files that need to be kept, and
	//   ignore - a list of directories which need to be ignored when traversing through
	//            the file tree
	//
	// All files that should be kept are listed in the manifest in the following format:
	//   ./Dir/Subdir/../file.ext
	// All directories that should be ignored are listed in the following format:
	//   ignore ./Dir/Subdir/../
	// All top-level directories that needs to be considered are listed in the following format:
	//   toplevel ./Dir
	//
	// The Manifest file is generated via $ sh Scripts/make_manifest.sh
	internal class Custodian
	{
		private string manifestPath;

		#region ManifestDirective
		private class ManifestDirective
		{
			private const string ignore = "ignore";
			private const string toplevel = "toplevel";
	
			private ManifestDirective(string keyword) { Keyword = keyword; }

			public string Keyword { get; set; }
			public string KeywordPrefix { get { return Keyword + " ."; } }

			public static ManifestDirective Ignore {
				get { return new ManifestDirective (ignore); }
			}
			public static ManifestDirective TopLevel {
				get { return new ManifestDirective (toplevel); }
			}
		}
		#endregion

		#region ManifestIndex
		private class ManifestIndex
		{
			// Files/directories that need to be kept
			private HashSet<string> toKeep;
			// Files/directories that should be ignored during traversal
			private HashSet<string> toIgnore;
			// Since traversal starts at a root-level directory which contains
			// files that are potentially not ours, this dictates which directories
			// to consider during traversal
			private HashSet<string> topLevel;
			public HashSet<string> TopLevel {
				get { return topLevel; }
			}

			public ManifestIndex()
			{
				toKeep = new HashSet<string> ();
				toIgnore = new HashSet<string> ();
				topLevel = new HashSet<string> ();
			}

			public bool Kept(string file)
			{
				return toKeep.Contains (file);
			}

			public bool Ignored(string file)
			{
				return toIgnore.Contains (file);
			}

			private void IndexLine(string line)
			{
				if (line.StartsWith (ManifestDirective.Ignore.Keyword)) {
					toIgnore.Add (FileUtils.PathRelativeToRoot (
						FileUtils.Root,
						line.Substring (ManifestDirective.Ignore.KeywordPrefix.Length)
					));
					return;
				}
				
				if (line.StartsWith (ManifestDirective.TopLevel.Keyword)) {
					topLevel.Add (FileUtils.NormalizePathForPlatform (
						line.Substring (ManifestDirective.TopLevel.KeywordPrefix.Length)
					));
					return;
				}
				
				toKeep.Add (FileUtils.PathRelativeToRoot (
					FileUtils.Root,
					line.Substring (".".Length)
				));
			}

			public static ManifestIndex FromFile(string manifestPath, string root)
			{
				ManifestIndex index = new ManifestIndex ();
				
				foreach (string line in File.ReadAllLines (manifestPath)) {
					index.IndexLine (line);
				}
				
				return index;
			}
		}
		#endregion

		#region Descriptor
		private class Descriptor : IComparable<Descriptor>
		{
			public enum FileType
			{
				File,
				Directory
			}

			public string Path { get; set; }
			public FileType Type { get; set; }

			public int CompareTo(Descriptor other)
			{
				return -this.Path.CompareTo (other.Path);
			}
		}
		#endregion

		public Custodian(string manifestPathRelativeToDataPath)
		{
			this.manifestPath = FileUtils.NormalizePathForPlatform (Path.Combine (
				FileUtils.Root,
				manifestPathRelativeToDataPath
			));
		}

		public IEnumerator Clean()
		{
			while (EditorApplication.isCompiling) {
				yield return null;
			}

			Clean (manifestPath, FileUtils.Root);
		}

		#region Details
		private static void Clean(string manifestPath, string root)
		{
			if (!File.Exists (manifestPath)) {
				Utils.Warn ("File manifest is absent, nothing to clean");
				return;
			}

			Utils.Log ("Tidying up...");
			CleanFileTree (manifestPath, root);
			AssetDatabase.Refresh ();
			Utils.Log ("Done.");
		}

		private static bool IsHidden(FileInfo file)
		{
#if UNITY_EDITOR_WIN
			// The hidden attribute doesn't work in the same way on Windows as it does on OSX.
			// Since the purpose of this function is to ignore .DS_Store files, we can always
			// return false for Windows.
			return false;
#else
			return (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
#endif
		}

		private static void HandleFile(FileInfo file, ManifestIndex index, ref List<Descriptor> toDelete)
		{
			string absolute = FileUtils.NormalizePathForPlatform (file.FullName);

			if (index.Ignored (absolute)) {
				Utils.Log ("Ignoring {0}", absolute);
				return;
			}

			if (IsHidden (file)) {
				return;
			}

			if (!index.Kept (absolute)) {
				toDelete.Add (new Descriptor {
					Path = absolute,
					Type = Descriptor.FileType.File
				});
			}
		}

		private static void HandleDirectory(string directory, ManifestIndex index, ref List<Descriptor> toDelete)
		{
			if (index.Ignored (directory)) {
				Utils.Log ("Ignoring {0}", directory);
				return;
			}

			if (!index.Kept (directory)) {
				toDelete.Add (new Descriptor {
					Path = directory,
					Type = Descriptor.FileType.Directory
				});
			}

			toDelete.AddRange (CollectForDeletion (directory, index));
		}

		// Collects a list of files and directories that need to be deleted
		private static List<Descriptor> CollectForDeletion(string path, ManifestIndex index)
		{
			List<Descriptor> toDelete = new List<Descriptor> ();
			DirectoryInfo directory = new DirectoryInfo (path);
			
			foreach (FileInfo file in directory.GetFiles ()) {
				HandleFile (file, index, ref toDelete);
			}
			
			foreach (string dir in Directory.GetDirectories (path)) {
				HandleDirectory (FileUtils.NormalizePathForPlatform (dir), index, ref toDelete);
			}
			
			return toDelete;
		}

		private static List<Descriptor> CollectForDeletion(string manifest, string root)
		{
			ManifestIndex index = ManifestIndex.FromFile (manifest, root);
			List<Descriptor> toDelete = new List<Descriptor> ();

			foreach (string topLevel in index.TopLevel) {
				toDelete.AddRange (CollectForDeletion (root + topLevel, index));
			}

			return toDelete;
		}

		private static List<Descriptor> Filter(List<Descriptor> list, Descriptor.FileType type)
		{
			List<Descriptor> paths = list.FindAll (file => file.Type == type);
			paths.Sort ();
			return paths;
		}

		private static void Invoke(string path, System.Action deleter)
		{
			try {
				deleter ();
			} catch (System.Exception e) {
				Utils.Log ("Couldn't delete {0} ({1}), skipping...", path, e.Message);
			}
		}

		private static void Delete(List<Descriptor> files)
		{
			// Delete files first, then directories, to prevent "directory $ is not empty" errors.
			foreach (Descriptor file in Filter (files, Descriptor.FileType.File)) {
				Invoke (file.Path, () => {
					File.Delete (file.Path);
					Utils.Log ("Deleted file {0}", file.Path);
				});
			}

			foreach (Descriptor file in Filter (files, Descriptor.FileType.Directory)) {
				Invoke (file.Path, () => {
					Directory.Delete (file.Path);
					Utils.Log ("Deleted directory {0}", file.Path);
				});
			}
		}

		private static void CleanFileTree(string manifest, string root)
		{
			Delete (CollectForDeletion (manifest, root));
		}
		#endregion
	}
}