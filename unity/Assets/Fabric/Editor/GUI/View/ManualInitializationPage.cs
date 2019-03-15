namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System;

	public class ManualInitializationPage : Page
	{
		private const int Selected = 0;
		private const int Deselected = -1;

		private readonly KeyValuePair<string, Action> back;
		private readonly KeyValuePair<string, Action> apply;
		private int selectedRadioId = 0;
		private Action onLinkClick;

		public ManualInitializationPage(
			Action onBack,
			Action<Internal.Editor.Model.Settings.InitializationType> onApply,
			Action onLinkClick,
			Func<Internal.Editor.Model.Settings.InitializationType> currentInitializationType
		)
		{
			back = new KeyValuePair<string, Action> ("Back", onBack);
			apply = new KeyValuePair<string, Action> ("Apply", () => {
				onApply (selectedRadioId == 0 ?
					Internal.Editor.Model.Settings.InitializationType.Automatic :
					Internal.Editor.Model.Settings.InitializationType.Manual
				);
				onBack ();
			});
			this.onLinkClick = onLinkClick;

			switch (currentInitializationType ()) {
			case Internal.Editor.Model.Settings.InitializationType.Automatic:
				selectedRadioId = 0;
				break;
			case Internal.Editor.Model.Settings.InitializationType.Manual:
				selectedRadioId = 1;
				break;
			}
		}

		#region Components
		private static class Components
		{
			private static readonly GUIStyle RadioStyle;
			private static readonly GUIStyle RadioGroupStyle;
			private static readonly GUIStyle DescriptionStyle;
			private static readonly GUIStyle SnippetCaptionStyle;
			private static readonly GUIStyle CodeStyle;
			private static readonly GUIStyle CodeGroupStyle;
			private static readonly GUIContent TextContent;
			private static readonly Texture2D Background = View.Render.MakeBackground (1, 1, View.Render.DBlue);
			private static readonly GUIStyle SeparatorStyle;
			private static readonly GUIStyle LinkStyle;

			private static readonly Color32 LinkNormalColor = Fabric.Internal.Editor.View.Render.FromHex (0x2B6591);

			static Components()
			{
				RadioStyle = new GUIStyle (EditorStyles.radioButton);
				RadioStyle.normal.textColor = Color.white;
				RadioStyle.onNormal.textColor = Color.white;
				RadioStyle.onActive.textColor = Color.white;
				RadioStyle.onHover.textColor = Color.white;
				RadioStyle.onFocused.textColor = Color.white;
				RadioStyle.fontStyle = FontStyle.Bold;
				RadioStyle.fontSize = 13;
				RadioStyle.wordWrap = true;
				RadioStyle.padding = new RectOffset (20, 20, 2, 10);

				RadioGroupStyle = new GUIStyle ();
				RadioGroupStyle.padding = new RectOffset (20, 10, 10, 0);

				DescriptionStyle = new GUIStyle ();
				DescriptionStyle.normal.textColor = Color.white;
				DescriptionStyle.wordWrap = true;
				DescriptionStyle.padding = new RectOffset (40, 20, 0, 2);
				DescriptionStyle.fontSize = 13;

				SnippetCaptionStyle = new GUIStyle (DescriptionStyle);
				SnippetCaptionStyle.padding = new RectOffset (0, 0, 0, 15);

				CodeStyle = new GUIStyle (GUI.skin.textField);
				CodeStyle.normal.textColor = Color.white;
				CodeStyle.active.textColor = Color.white;
				CodeStyle.focused.textColor = Color.white;
				CodeStyle.normal.background = Background;
				CodeStyle.active.background = Background;
				CodeStyle.focused.background = Background;

				CodeStyle.padding = new RectOffset (0, 0, 0, 0);
				CodeStyle.fontSize = 13;
				CodeStyle.fontStyle = FontStyle.Bold;
				CodeStyle.margin = new RectOffset (0, 0, 0, 2);
				CodeStyle.padding = new RectOffset (8, 8, 8, 8);

				CodeGroupStyle = new GUIStyle ();
				CodeGroupStyle.margin = new RectOffset (40, 20, 20, 15);

				TextContent = new GUIContent ();

				SeparatorStyle = new GUIStyle ();
				SeparatorStyle.fixedHeight = 1;
				SeparatorStyle.normal.background = View.Render.MakeBackground (1, 1, new Color32 (255, 255, 255, 76));
				SeparatorStyle.margin = new RectOffset (0, 0, 15, 5);

				LinkStyle = new GUIStyle (GUI.skin.label);
				LinkStyle.normal.textColor = LinkNormalColor;
				LinkStyle.hover.textColor = Color.white;
				LinkStyle.fontSize = 14;
				LinkStyle.wordWrap = true;
				LinkStyle.padding = new RectOffset (0, 0, 10, 0);
			}

			public static void RenderOptions(ref int selectedRadioId, int id, string text)
			{
				GUILayout.BeginVertical (RadioGroupStyle);

				if (GUILayout.SelectionGrid (
					selectedRadioId == id ? Selected : Deselected, new string[] { text }, 1, RadioStyle
				) == Selected) {
					selectedRadioId = id;
				}

				GUILayout.EndVertical ();
			}

			public static void RenderDescription(string description)
			{
				GUILayout.BeginVertical ();
				GUILayout.Label (description, DescriptionStyle);
				GUILayout.EndVertical ();
			}

			public static void RenderSnippet(string caption, string code)
			{
				GUILayout.BeginVertical (CodeGroupStyle);
				GUILayout.Label (caption, SnippetCaptionStyle);

				TextContent.text = code;
				Rect fieldRect = GUILayoutUtility.GetRect (TextContent, CodeStyle);
				EditorGUI.SelectableLabel (fieldRect, code, CodeStyle);

				GUILayout.EndVertical ();
			}

			public static void RenderSeparator()
			{
				GUILayout.BeginVertical ();
				GUILayout.Label ("", SeparatorStyle);
				GUILayout.EndVertical ();
			}

			public static void RenderLink(string caption, Action onClick)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.BeginVertical ();

				if (GUILayout.Button (caption, LinkStyle)) {
					onClick ();
				}
				EditorGUIUtility.AddCursorRect (GUILayoutUtility.GetLastRect (), MouseCursor.Link);

				GUILayout.EndVertical ();
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}
		}
		#endregion

		public override void RenderImpl(Rect position)
		{
			RenderHeader ("Configure Initialization");
			Components.RenderOptions (ref selectedRadioId, 0, "Automatic (Recommended)");
			Components.RenderDescription ("This option will initialize Fabric kits as early as possible in the application startup cycle.");
			Components.RenderSeparator ();
			Components.RenderOptions (ref selectedRadioId, 1, "Manual");
			Components.RenderDescription (
				"This option will defer initialization of Fabric kits until the code snippet below is called. This is <b>not</b> recommended for a majority of users."
			);
			Components.RenderSnippet (
				"After selecting manual initialization, copy and paste the following code into your application, then press <b>apply</b> to continue.",
				"Fabric.Initialize();"
			);
			Components.RenderLink ("Learn More", onLinkClick);
			RenderFooter (back, apply);
		}
	}
}
