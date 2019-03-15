namespace Fabric.Internal.Editor.View.Templates
{
	using UnityEditor;
	using UnityEngine;
	using System.Collections.Generic;
	using System;
	using Fabric.Internal.Editor.View;
	
	public class PrefabPage : Page
	{
		private const string FabricPrefabName = "FabricInit";

		private KeyValuePair<string, Action> next;
		private string prefabName;
		private System.Type prefabScript;

		private EditorApplication.HierarchyWindowItemCallback acceptDragAndDrop = null;

		public PrefabPage(Action onNextClicked, string prefabName, System.Type prefabScript)
		{
			this.next = new KeyValuePair<string, Action> ("Next", delegate() {
				RemoveListener ();
				onNextClicked ();
			});
			this.prefabName = prefabName;
			this.prefabScript = prefabScript;
		}

		private void RemoveListener()
		{
			if (acceptDragAndDrop != null) {
				EditorApplication.hierarchyWindowItemOnGUI -= acceptDragAndDrop;
			}
		}

		#region Components
		private static class Components
		{
			private static GUIStyle IconStyle;
			private static readonly GUIStyle BorderStyle;
			private static readonly GUIStyle MessageStyle;
			private static readonly Texture2D PrefabIcon = Images.Loader.Load ("prefab-box.png");

			private static bool pressed = false;
			private static Rect PrefabIconRect = new Rect (0, 150, 192, 192);

			static Components()
			{
				IconStyle = new GUIStyle (GUI.skin.button);
				BorderStyle = new GUIStyle (GUI.skin.label);
				MessageStyle = new GUIStyle ();

				IconStyle.fixedWidth = 192;
				IconStyle.fixedHeight = 192;
				IconStyle.normal.background = null;
				IconStyle.hover.background = View.Render.MakeBackground (192, 192, View.Render.DBlue);
	
				BorderStyle.normal.background = View.Render.MakeBackground (200, 200, View.Render.DBlue);
				BorderStyle.normal.textColor = Color.white;
				BorderStyle.contentOffset = new Vector2 (2, 1);
				BorderStyle.fontStyle = FontStyle.Bold;

				MessageStyle.normal.textColor = Color.white;
				MessageStyle.fontSize = 14;
				MessageStyle.margin.left = 20;
				MessageStyle.wordWrap = true;
			}

			public static void Reposition(Rect position)
			{
				PrefabIconRect.x = position.width / 2 - PrefabIconRect.width / 2;
				View.Render.Center (position, IconStyle);
			}

			public static void RenderMessage (string message)
			{
				EditorGUILayout.Space ();
				GUILayout.Label (message, MessageStyle);
			}

			private static float CurrentIconAngle()
			{
				return 5.0f * (float) Math.Sin (6.0f * Math.PI * Time.realtimeSinceStartup);
			}

			public static void RenderIcon()
			{
				float angle = CurrentIconAngle ();

				GUIUtility.RotateAroundPivot (angle, PrefabIconRect.center);
				GUI.Label (PrefabIconRect, PrefabIcon, IconStyle);
				GUIUtility.RotateAroundPivot (-angle, PrefabIconRect.center);
			}

			public static void PrepareDragAndDrop(string prefabName, EditorApplication.HierarchyWindowItemCallback acceptDragAndDrop, Action removeListener)
			{
				if (PrefabIconRect.Contains (Event.current.mousePosition)) {
					if (Event.current.type == EventType.MouseDown) {
						pressed = true;
					}
					if (Event.current.type == EventType.MouseUp) {
						pressed = false;
					}
				}

				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				Event current = Event.current;
				if (pressed && current.type == EventType.MouseDrag) {
					DragAndDrop.PrepareStartDrag ();
					DragAndDrop.paths = null;
					DragAndDrop.objectReferences = new UnityEngine.Object[0];
					DragAndDrop.SetGenericData ("object", Event.current);
					DragAndDrop.StartDrag (prefabName);

					removeListener ();
					EditorApplication.hierarchyWindowItemOnGUI += acceptDragAndDrop;

					current.Use ();
				}
			}

			private static bool PrefabAlreadyExists(string name)
			{
				foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType (typeof (GameObject))) {
					if (obj.name.StartsWith (name)) {
						return true;
					}
				}

				return false;
			}

			private static void AddPrefab(string name, System.Type type)
			{
				if (PrefabAlreadyExists (name)) {
					Fabric.Internal.Editor.Utils.Log ("{0} already exists in this scene", name);
					return;
				}

				GameObject gameObject = new GameObject (name);
				System.Reflection.MethodInfo addComponent = typeof(GameObject).GetMethod ("AddComponent", new System.Type[] {});
				System.Reflection.MethodInfo addComponentGeneric = addComponent.MakeGenericMethod (type);
				addComponentGeneric.Invoke (gameObject, null);
			}

			private static void MarkSceneDirty(string prefabName)
			{
				GameObject prefab = GameObject.Find (prefabName);

				if (prefab == null) {
					Fabric.Internal.Editor.Utils.Log ("Couldn't find the {0} Prefab in any scene", prefabName);
					return;
				}
#if UNITY_5_3_OR_NEWER
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty (prefab.scene);
#else
				EditorUtility.SetDirty (prefab);
#endif
			}

			public static void AcceptDragAndDrop(string prefabName, System.Type prefabScript, Action removeListener, Action advance)
			{
				if (Event.current.type == EventType.DragUpdated) {
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				}

				if (Event.current.type != EventType.DragPerform) {
					return;
				}

				DragAndDrop.AcceptDrag ();
				AddPrefab (FabricPrefabName, typeof(Fabric.Internal.FabricInit));
				AddPrefab (prefabName, prefabScript);
				removeListener ();

				MarkSceneDirty (prefabName);

				advance ();
			}
		}
		#endregion

		public override void RenderImpl(Rect position)
		{
			RenderHeader ("You're almost there...");
			RenderFooter (null, next);
			Components.Reposition (position);
			Components.RenderMessage ("1) Open your <b>first</b> scene");
			Components.RenderMessage ("2) Drag and drop the game object below into the game object hierarchy");
			Components.RenderIcon ();

			if (acceptDragAndDrop == null) {
				acceptDragAndDrop = delegate(int arg1, Rect arg2) {
					Components.AcceptDragAndDrop (prefabName, prefabScript, RemoveListener, next.Value);
				};
			}

			Components.PrepareDragAndDrop (prefabName, acceptDragAndDrop, RemoveListener);
		}
	}
}