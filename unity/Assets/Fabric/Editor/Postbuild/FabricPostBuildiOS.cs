namespace Fabric.Internal.Editor.Postbuild
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using Fabric.Internal.Editor.Model;
	using Fabric.Internal.Editor.ThirdParty.xcodeapi;

	public class FabricPostBuildiOS : PostBuildiOS
	{

		[PostProcessBuild(100)]
		public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath) {
			// BuiltTarget.iOS is not defined in Unity 4, so we just use strings here
			if (buildTarget.ToString () == "iOS" || buildTarget.ToString () == "iPhone") {
				PrepareProject (buildPath);
				PreparePlist (buildPath);
			}
		}
		
		private static void PrepareProject (string buildPath)
		{
			Settings settings = Settings.Instance;
			string projPath = Path.Combine (buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");

			if (string.IsNullOrEmpty(settings.Organization.ApiKey) || string.IsNullOrEmpty(settings.Organization.BuildSecret)) {
				Utils.Error ("Unable to find API Key or Build Secret. Fabric was not added to the player.");
				return;
			}

			AddFabricRunScriptBuildPhase (projPath);
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			AddFabricFrameworkSearchPath (projPath);
#endif
		}
		
		private static void PreparePlist (string buildPath)
		{
			Dictionary<string, PlistElementDict> kitsDict = new Dictionary<string, PlistElementDict>();			
			AddFabricKitsToPlist (buildPath, kitsDict);
			SetInitializationTypePlistFlag (buildPath);
			SetInitializationPlistKitsList (buildPath);
		}

		private static void ModifyFabricPlistElement(string buildPath, System.Action<PlistElementDict> modify)
		{
			string plistPath = Path.Combine (buildPath, "Info.plist");

			PlistDocument plist = new PlistDocument ();
			plist.ReadFromFile (plistPath);

			PlistElementDict rootDict = plist.root.AsDict ();
			PlistElementDict fabric = (PlistElementDict)rootDict ["Fabric"] ?? plist.root.CreateDict ("Fabric");

			modify (fabric);

			plist.WriteToFile (plistPath);
		}

		private static void SetInitializationTypePlistFlag(string buildPath)
		{
			ModifyFabricPlistElement (buildPath, delegate(PlistElementDict obj) {
				obj.SetString ("InitializationType", Settings.Instance.Initialization.ToString ());
			});
		}

		private static void SetInitializationPlistKitsList(string buildPath)
		{
			if (Settings.Instance.Initialization == Settings.InitializationType.Automatic || Settings.Instance.InstalledKits.Count == 0) {
				return;
			}

			ModifyFabricPlistElement (buildPath, delegate(PlistElementDict obj) {
				obj.SetString ("InitializationKitsList", CommonBuild.FabricCommonBuild.InitializationKitsList ());
			});
		}

		private static void AddFabricFrameworkSearchPath (string projPath)
		{
			PBXProject project = new PBXProject();
			project.ReadFromString(File.ReadAllText(projPath));
			string target = project.TargetGuidByName("Unity-iPhone");

			Utils.Log ("Adding Framework Search Paths to Xcode project");
			project.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(SRCROOT)/Frameworks/" + fabricPluginsPath);

			File.WriteAllText(projPath, project.WriteToString());
		}

		private static void AddFabricRunScriptBuildPhase (string projPath)
		{
			var xcodeProjectLines = File.ReadAllLines (projPath);
			foreach (var line in xcodeProjectLines) {
				if (line.Contains("Fabric.framework/run"))
					return;
			}
			
			var settings = Settings.Instance;
			var scriptUUID = System.Guid.NewGuid ().ToString ("N").Substring (0, 24).ToUpper ();
			var inBuildPhases = false;
			var sb = new StringBuilder ();			
			
			Utils.Log ("Adding Fabric.framework/run Run Script Build Phase to Xcode project");
			
			var hasScriptBuildPhase = false;
			foreach (var line in xcodeProjectLines) {
				if (line.Contains ("/* Begin PBXShellScriptBuildPhase section */")) {
					hasScriptBuildPhase = true;
				}
			}
			
			string shellScriptLines = 
				"\t\t" + scriptUUID + " /* ShellScript */ = {\n" +
					"\t\t\tisa = PBXShellScriptBuildPhase;\n" +
					"\t\t\tbuildActionMask = 2147483647;\n" +
					"\t\t\tfiles = (\n" +
					"\t\t\t);\n" +
					"\t\t\tinputPaths = (\n" +
					"\t\t\t);\n" +
					"\t\t\toutputPaths = (\n" +
					"\t\t\t);\n" +
					"\t\t\trunOnlyForDeploymentPostprocessing = 0;\n" +
					"\t\t\tshellPath = \"/bin/sh -x\";\n" +
					"\t\t\tshellScript = \"" +
					"chmod u+x ./Frameworks/" + fabricPluginsPath + "/Fabric.framework/run\n" +
					"chmod u+x ./Frameworks/" + fabricPluginsPath + "/Fabric.framework/uploadDSYM\n" +
					"./Frameworks/" + fabricPluginsPath + "/Fabric.framework/run " + settings.Organization.ApiKey + " " + settings.Organization.BuildSecret + " --skip-check-update\";\n" +
					"\t\t};\n";
			
			foreach (var line in xcodeProjectLines) {
				if (hasScriptBuildPhase && line.Contains ("/* Begin PBXShellScriptBuildPhase section */")) {
					sb.AppendLine (line);				
					sb.Append (shellScriptLines);
				} else if (!hasScriptBuildPhase && line.Contains ("/* End PBXResourcesBuildPhase section */")) {
					sb.AppendLine (line);
					sb.AppendLine ("/* Begin PBXShellScriptBuildPhase section */");
					sb.Append (shellScriptLines);
					sb.AppendLine ("/* End PBXShellScriptBuildPhase section */");
				} else if (line.Contains ("buildPhases = (")) {
					inBuildPhases = true;
					sb.AppendLine(line);
				} else if (inBuildPhases && line.Contains(");")) {
					inBuildPhases = false;
					sb.AppendLine ("\t\t\t\t" + scriptUUID + " /* ShellScript */,");
					sb.AppendLine (line);
					
				} else {
					sb.AppendLine(line);
				}
				
			}
			
			File.WriteAllText(projPath, sb.ToString());
		}

	}

}
