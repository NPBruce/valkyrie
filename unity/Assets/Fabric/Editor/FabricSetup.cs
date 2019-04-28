
namespace Fabric.Internal.Editor
{
	using System;
	using UnityEditor;
	using System.IO;
	using UnityEngine;
	using System.Xml;
	using Fabric.Internal.Editor.Model;
	using System.Collections.Generic;

	public class FabricSetup
	{
		private static readonly int kitScriptPriority = -200;
		private static readonly int fabricScriptPriority = -100;

		private static readonly string templateManifestRelativePath = "Plugins/Android/fabric/Template-AndroidManifest.xml";
		private static readonly string templateManifestPath = Update.FileUtils.NormalizePathForPlatform (Path.Combine(
			Application.dataPath,
			templateManifestRelativePath
		));

		private static readonly string fabricManifestRelativePath = "Plugins/Android/fabric/AndroidManifest.xml";
		private static readonly string fabricManifestPath = Update.FileUtils.NormalizePathForPlatform (Path.Combine(
			Application.dataPath,
			fabricManifestRelativePath
		));

		private static readonly string topLevelManifestRelativePath = "Plugins/Android/AndroidManifest.xml";
		private static readonly string topLevelManifestPath = Update.FileUtils.NormalizePathForPlatform (Path.Combine(
			Application.dataPath,
			topLevelManifestRelativePath
		));
		
		private static readonly string fabricApplicationName = "io.fabric.unity.android.FabricApplication";

		private static readonly string[] UnityManifestPaths = { 
			// Unity <5.2
			Update.FileUtils.NormalizePathForPlatform (Path.Combine(
				EditorApplication.applicationContentsPath,
				"PlaybackEngines/AndroidPlayer/AndroidManifest.xml"
			)),
			// Unity 5.2
			Update.FileUtils.NormalizePathForPlatform (Path.Combine(
				EditorApplication.applicationContentsPath,
				"PlaybackEngines/AndroidPlayer/Apk/AndroidManifest.xml"
			)),
			// Unity 5.3
			Update.FileUtils.NormalizePathForPlatform (Path.Combine(
				EditorApplication.applicationPath,
				"../PlaybackEngines/AndroidPlayer/Apk/AndroidManifest.xml"
			))
		};
		
		#region Top-level AndroidManifest

		protected static string FindUnityAndroidManifest()
		{
			return Array.Find (UnityManifestPaths, (manifestPath) => File.Exists (manifestPath));
		}

		protected static void BootstrapTopLevelManifest()
		{
			string unityManifestPath = FindUnityAndroidManifest ();

			if (unityManifestPath == null) {
				Utils.Error ("Could not find Unity's AndroidManifest.xml file");
				return;
			}

			BootstrapTopLevelManifest (unityManifestPath);
		}

		protected static void BootstrapTopLevelManifest(string inputManifestPath)
		{			
			if (!File.Exists (topLevelManifestPath)) {
				Utils.Log ("Writing {0}", topLevelManifestRelativePath);
				File.Copy(inputManifestPath, topLevelManifestPath);
			}
		}

		protected static void ToggleApplicationInTopLevelManifest (bool enableFabric) {
			XmlDocument doc = new XmlDocument();		
			doc.Load(topLevelManifestPath);
			if (doc == null) {
				Utils.Error ("Could not open {0}", topLevelManifestRelativePath);
				return;
			}

			var applicationNodes = doc.GetElementsByTagName("application");
			if (applicationNodes.Count != 1) {
				Utils.Error ("Manifest does not have one (and only one) <application> tag: {0}", topLevelManifestRelativePath);
				return;
			}		
			var applicationNode = applicationNodes [0];
			var androidNs = applicationNode.GetNamespaceOfPrefix("android");

			if (enableFabric) {
				Utils.Log ("Enabling Fabric in: {0}", topLevelManifestRelativePath);
				var applicationNameAttr = doc.CreateNode(XmlNodeType.Attribute, "name", androidNs);
				applicationNameAttr.Value = fabricApplicationName;
				applicationNode.Attributes.SetNamedItem(applicationNameAttr);
			} else {
				var applicationNameAttr = applicationNode.Attributes.GetNamedItem("name", androidNs);
				if (applicationNameAttr != null && applicationNameAttr.Value == fabricApplicationName) {
					Utils.Log ("Disabling Fabric in: {0}", topLevelManifestRelativePath);			
					applicationNode.Attributes.RemoveNamedItem("name", androidNs);
				}
			}

			doc.Save (topLevelManifestPath);
		}

		#endregion

		#region Fabric's AndroidManifest

		private static void ManipulateMetadataElementInFabricManifest (string key, Action<XmlElement, XmlDocument, XmlNode, string> manipulator)
		{
			if (!File.Exists (fabricManifestPath)) {
				File.Copy(templateManifestPath, fabricManifestPath);
			}

			XmlDocument doc = new XmlDocument();			
			doc.Load(fabricManifestPath);
			
			if (doc == null) {
				Utils.Error ("Could not open {0}", fabricManifestRelativePath);
				return;
			}

			// Get android namespace
			XmlNodeList applicationNodes = doc.GetElementsByTagName("application");
			if (applicationNodes.Count < 1) {
				Utils.Error ("Could not find <application> tag in {0}", fabricManifestRelativePath);
				return;
			}

			XmlNode applicationNode = applicationNodes [0];
			string androidNs = applicationNode.GetNamespaceOfPrefix("android");			
			var metaElements = doc.GetElementsByTagName("meta-data");
			XmlElement element = null;

			foreach (XmlElement metaElement in metaElements) {
				if (metaElement.GetAttribute("name", androidNs) == key) {
					element = metaElement;
				}
			}

			manipulator(element, doc, applicationNode, androidNs);

			doc.Save (fabricManifestPath);
		}

		protected static void InjectMetadataIntoFabricManifest (string key, string value, bool add = false)
		{
			ManipulateMetadataElementInFabricManifest (key, (XmlElement element, XmlDocument doc, XmlNode applicationNode, string androidNs) => {
				string newValue = "";

				if (element != null) {
					string currentValue = element.GetAttribute("value", androidNs);
					
					if (currentValue == value || (add && currentValue.Contains (value))) {
						Utils.Log (string.Format("Metadata with key {0} already added to {1}", key, fabricManifestRelativePath));
						return;
					}
					
					element.ParentNode.RemoveChild(element);
					newValue = currentValue;
				}

				Utils.Log ("Adding metadata with key {0} to {1}", key, fabricManifestRelativePath);
				var metaNode = doc.CreateElement ("meta-data");

				if (add) {
					newValue += (newValue.Length == 0 ? "" : ",") + value;
				} else {
					newValue = value;
				}
				
				metaNode.SetAttribute ("name", androidNs, key);
				metaNode.SetAttribute ("value", androidNs, newValue);				
				applicationNode.AppendChild (metaNode);
			});
		}
		
		protected static void RemoveMetadataFromFabricManifest (string key, string valueToBeRemoved = null) {
			ManipulateMetadataElementInFabricManifest (key, (XmlElement element, XmlDocument doc, XmlNode applicationNode, string androidNs) => {
				if (File.Exists (fabricManifestRelativePath)) {
	 				Utils.Log ("Removing metadata with key {0} in {1}", key, fabricManifestRelativePath);

					string elementValue = element.GetAttribute ("value", androidNs);
					if (valueToBeRemoved != null && elementValue.Contains (valueToBeRemoved)) {
						// Find and remove what needs to be removed in the comma-separated list
						List<string> values = new List<string> (elementValue.Split (','));
						values.RemoveAt (values.IndexOf (valueToBeRemoved));
						string newValue = string.Join (",", values.ToArray ());
						element.SetAttribute ("value", androidNs, newValue);
					}
				}
			});
		}
			
		#endregion

		#region Other

		private static void SetScriptExecutionOrder (Type scriptClass, int priority)
		{
			string libName = scriptClass.Name;

			foreach (MonoScript script in MonoImporter.GetAllRuntimeMonoScripts()) {
				if (script.name == libName) {
					if (MonoImporter.GetExecutionOrder(script) != priority) {
						Utils.Log ("Changing script execution order for {0}", libName);
						MonoImporter.SetExecutionOrder(script, priority);
					} else {
						Utils.Log ("Script execution order for {0} already set", libName);
					}
					break;
				}
			}
		}
	
		protected static void SetKitScriptExecutionOrder (Type kitScriptClass)
		{
			SetScriptExecutionOrder (typeof(Fabric.Internal.FabricInit), fabricScriptPriority);
			SetScriptExecutionOrder (kitScriptClass, kitScriptPriority);
		}

		#endregion
	}
	
}