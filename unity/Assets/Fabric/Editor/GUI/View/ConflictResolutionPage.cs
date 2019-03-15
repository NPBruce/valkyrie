namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using System.Collections.Generic;
	using System;
	using KitsList = System.Collections.Generic.List<Update.Dependency.DependencyGraphObject.DependenciesObject.KitsObject>;
	using KitsObject = Fabric.Internal.Editor.Update.Dependency.DependencyGraphObject.DependenciesObject.KitsObject;
	using Fabric.Internal.Editor.Model;

	internal class ConflictResolutionPage : Page
	{
		private const string MessageText = "<b>{0}</b> shares a common dependency with {1}other kit{2} you have installed. Quickly update the existing kit{2} below before installing <b>{0}</b>!";

		private KeyValuePair<string, Action> back;
		private Action<KitsObject, ImportedKit, bool> onKitSelected;
		private Func<string> toInstall;
		private Func<string, HashSet<string>> toUpdate;
		private KitsList availableKits;
		private HashSet<string> KitsToUpdate
		{
			get {
				return toUpdate (toInstall ());
			}
		}

		public ConflictResolutionPage(
			Action<string> onBack,
			Action<KitsObject, ImportedKit, bool> onKitSelected,
			KitsList availableKits,
			Func<string> toInstall,
			Func<string, HashSet<string>> toUpdate
		)
		{
			this.back = new KeyValuePair<string, Action> ("Back", () => { onBack (""); });
			this.onKitSelected = onKitSelected;
			this.availableKits = availableKits;
			this.toInstall = toInstall;
			this.toUpdate = toUpdate;
		}

		#region Components
		private static class Components
		{
			private static readonly GUIStyle MessageStyle;
			private static readonly GUIStyle ScrollStyle;
			private static readonly GUIStyle RowStyle;
			private static Vector2 scroll;

			private static readonly Color32 SelectedColor = View.Render.LBlue;
			private static readonly Color32 RowColor = View.Render.FromHex (0x2B6591);

			static Components()
			{
				MessageStyle = new GUIStyle ();
				MessageStyle.normal.textColor = Color.white;
				MessageStyle.wordWrap = true;
				MessageStyle.padding = new RectOffset (20, 20, 10, 0);
				MessageStyle.fontSize = 13;

				ScrollStyle = new GUIStyle (GUI.skin.scrollView);
				ScrollStyle.margin = new RectOffset (18, 16, 18, 0);

				RowStyle = new GUIStyle (GUI.skin.button);
				RowStyle.padding = new RectOffset (14, 14, 14, 14);
				RowStyle.alignment = TextAnchor.MiddleLeft;
				RowStyle.fontSize = 14;
				RowStyle.normal.textColor = Color.white;
				RowStyle.richText = true;

				int rowHeight = RowStyle.normal.background.height;
				int rowWidth = RowStyle.normal.background.width;

				RowStyle.normal.background = View.Render.MakeBackground (rowWidth, rowHeight, RowColor);
				RowStyle.hover.background = View.Render.MakeBackground (rowWidth, rowHeight, SelectedColor);
			}

			public static void RenderMessage(string message)
			{
				GUILayout.BeginVertical ();
				GUILayout.Label (message, MessageStyle);
				GUILayout.EndVertical ();
			}

			public static void RenderUpdateList(HashSet<string> toUpdate, Action<KitsObject, ImportedKit, bool> onSelected, KitsList kitsList)
			{
				scroll = GUILayout.BeginScrollView (scroll, ScrollStyle);

				foreach (string kit in toUpdate) {
					if (GUILayout.Button (kit, RowStyle)) {
						KitsObject kitObject = kitsList.Find (k => k.Name.Equals (kit, StringComparison.OrdinalIgnoreCase));
						onSelected (kitObject, null, false);
					}
				}

				GUILayout.EndScrollView ();
			}
		}
		#endregion

		public override void RenderImpl(Rect position)
		{
			RenderHeader ("Resolve Dependency Conflict");
			Components.RenderMessage (populateMessage (toInstall (), KitsToUpdate.Count != 1));
			Components.RenderUpdateList (KitsToUpdate, onKitSelected, availableKits);
			RenderFooter (back, null);
		}

		private static string populateMessage (string kit, bool plural)
		{
			return string.Format (MessageText, kit, plural ? "" : "an", plural ? "s" : "");
		}
	}
}
