namespace Fabric.Internal.Editor.Postbuild
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using Fabric.Internal.Editor.Model;
	using Fabric.Internal.Editor.ThirdParty.xcodeapi;

	public class PostBuildiOS
	{
		protected static readonly string fabricPluginsPath = "Plugins/iOS/Fabric";

		protected static void PreparePlist (string buildPath, string kit)
		{
			Dictionary<string, PlistElementDict> kitsDict = new Dictionary<string, PlistElementDict> () {
				{ kit, new PlistElementDict () }
			};

			AddFabricKitsToPlist(buildPath, kitsDict);
		}

		// Takes the build path where Info.plist is located
		// and a Dictionary<string, PlistElementDict> (kitsInfo) where
		// key: the KitName and
		// value: a PlistElementDict containing the KitInfo
		protected static void AddFabricKitsToPlist (string buildPath, Dictionary<string, PlistElementDict> kitsInfo)
		{
			Utils.Log ("Preparing Info.plist");

			Settings settings = Settings.Instance;
			string plistPath = Path.Combine (buildPath, "Info.plist");
			
			PlistDocument plist = new PlistDocument();
			plist.ReadFromFile(plistPath);
			
			PlistElementDict rootDict = plist.root.AsDict ();
			PlistElementDict fabricEl = (PlistElementDict)rootDict["Fabric"];
			if (fabricEl == null) {
				fabricEl = plist.root.CreateDict("Fabric");
			}			
			fabricEl.SetString("APIKey", settings.Organization.ApiKey);
			
			PlistElementArray fabricKits = (PlistElementArray)fabricEl["Kits"];
			if (fabricKits == null) {
				fabricKits = fabricEl.CreateArray("Kits");
			}

			foreach (KeyValuePair<string, PlistElementDict> entry in kitsInfo) {
				Utils.Log ("Adding kit {0} to Info.plist", entry.Key);

				PlistElementDict kitDict = fabricKits.AddDict();
				kitDict.SetString("KitName", entry.Key);
				kitDict["KitInfo"] = entry.Value;
			}

			plist.WriteToFile(plistPath);
		}

		// Copy and add a framework (Link Phase) to a PBXProject
		//
		// PBXProject project: the project to modify
		// string target: the target project's GUID
		// string framework: the path to the framework to add
		// string projectPath: the path to add the framework in the project, relative to the project root
		private static void AddFrameworkToProject(PBXProject project, string target,
		                                                    string framework, string buildPath, string projectPath)
		{
			Fabric.Internal.Editor.Utils.DirectoryCopy(framework, Path.Combine(buildPath, projectPath), true);
			string guid = project.AddFile (projectPath, projectPath);
			project.AddFileToBuild(target, guid);
		}
		
		protected static void AddFrameworksToProject(string[] frameworks, string buildPath, PBXProject project, string target) 
		{
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			string frameworksDir = Path.Combine(Directory.GetCurrentDirectory (), "Assets/" + fabricPluginsPath);
			string pluginFrameworksDir = "Frameworks/" + fabricPluginsPath + "/";

			foreach (string framework in frameworks) 
			{
				string pluginFrameworkDir = pluginFrameworksDir + framework;
				if (!Directory.Exists (Path.Combine(buildPath, pluginFrameworkDir))) {
					Utils.Log ("Adding {0} to Xcode project", framework);
					
					AddFrameworkToProject(project, target, Path.Combine (frameworksDir, framework), buildPath,
					                                pluginFrameworkDir);
				}
			}
#else
			// Unity 5 and above should already take care of copying and linking non-platform frameworks
#endif
		}

		protected static void AddPlatformFrameworksToProject(string[] frameworks, PBXProject project, string target)
		{
			foreach (string framework in frameworks) {
				if (!project.HasFramework (framework)) {
					Utils.Log ("Adding {0} to Xcode project", framework);
					project.AddFrameworkToProject (target, framework, false);
				}
			}
		}
		
		protected static void AddPlatformLibsToProject(string[] libs, PBXProject project, string target)
		{
			foreach (string lib in libs) {
				string libGUID = project.AddFile ("usr/lib/" + lib, "Libraries/" + lib, PBXSourceTree.Sdk);
				project.AddFileToBuild (target, libGUID);
			}	
		}

		protected static void AddLibsToProject(string[] libs, PBXProject project, string target, string buildPath)
		{
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			string fabricPluginsFullPath = Path.Combine(Directory.GetCurrentDirectory (), "Assets/" + fabricPluginsPath);

			foreach (string lib in libs) {
				string libPathInProject = Path.Combine ("Libraries", Path.GetFileName (lib));
				string libFullPathInProject = Path.Combine (buildPath, libPathInProject);

				if (!File.Exists (libFullPathInProject)) {
					File.Copy (Path.Combine (fabricPluginsFullPath, lib),
					           libFullPathInProject);

					string libGUID = project.AddFile (libFullPathInProject, libPathInProject, PBXSourceTree.Absolute);
					project.AddFileToBuild (target, libGUID);
				}
			}
#else
			// Unity 5 and above should already take care of copying and linking non-platform frameworks
#endif
		}

		protected static void AddBuildProperty(PBXProject project, string target, string property, string value)
		{
			project.AddBuildProperty (target, property, value);
		}

		protected static bool IsKitOnboarded (string kitName)
		{
			var installedKits = Settings.Instance.InstalledKits;
			return installedKits.Exists (installedKit => installedKit.Name == kitName);
		}

		protected static void DirectoryCopy(string fro, string to, bool recurse)
		{
			Fabric.Internal.Editor.Utils.DirectoryCopy (fro, to, recurse);
		}
	}
}
