namespace Fabric.Internal.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using Fabric.Internal.Editor.View;

	public class Editor : EditorWindow
	{
		#region Instance
		private static Editor instance;
		private static Editor Instance
		{
			get {
				if (instance == null)
					instance = GetFabricEditorWindow () as Editor;
				return instance;
			}
		}
		#endregion

		[MenuItem("Fabric/Prepare Fabric", false, 0)]
		public static void Init ()
		{
			instance = GetFabricEditorWindow () as Editor;
		}

		private Page currentPage;

		[MenuItem("Fabric/Advanced/Manual Initialization...", false, 1)]
		public static void ShowManualInitializationPage()
		{
			Model.Settings.Instance.FlowSequence = 300;
			Instance.Show ();
		}

		[MenuItem("Fabric/Advanced/Manual Initialization...", true)]
		public static bool ValidateShowManualInitializationPage()
		{
			// Only enable the Manual Initialization flow from the Kit Selection page. This simplifies
			// the Back button logic for that flow.
			return
				Model.Settings.Instance.FlowSequence == 0 &&
				!string.IsNullOrEmpty (Model.Settings.Instance.Organization.ApiKey) &&
				string.IsNullOrEmpty (Model.Settings.Instance.Kit);
		}

		public void OnGUI ()
		{
			EditorGUI.DrawRect (new Rect (0, 0, position.width, position.height), View.Render.Lerped);
			if (Event.current.type == EventType.Layout) {
				currentPage = Controller.PluginController.Instance.PageFromState ();
			}
			currentPage.Render (Instance.position);
		}

		public void Update ()
		{
			Repaint ();
		}

		private static EditorWindow GetFabricEditorWindow ()
		{
			return EditorWindow.GetWindowWithRect(
				typeof (Editor),
				new Rect(100, 100, View.Render.InitialWindowWidth, View.Render.InitialWindowHeight),
				false,
				"Fabric"
			);
		}
	}
}
