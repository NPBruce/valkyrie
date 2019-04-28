namespace Fabric.Internal.Crashlytics.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using Fabric.Internal.Editor.View;
	
	public class DashboardPage : Page
	{
		private Func<Texture2D> DownloadIcon;
		private Func<string> DashboardUrl;
		private KeyValuePair<string, Action> back;

		public DashboardPage(Func<Texture2D> downloadIcon, Func<string> dashboardUrl, Action onBackClick)
		{
			this.DownloadIcon = downloadIcon;
			this.DashboardUrl = dashboardUrl;
			this.back = new KeyValuePair<string, Action> ("Back", onBackClick);
		}

		#region Components
		private static class Components
		{
			private static readonly GUIStyle IconStyle;
			private static readonly GUIStyle IconBackgroundStyle;
			private static readonly GUIStyle MessageStyle;
			private static readonly GUIStyle TextStyle;

			private static readonly Texture2D PlaceHolderImage;
			private static readonly Texture2D IconBackground;

			static Components()
			{
				PlaceHolderImage = Fabric.Internal.Editor.Images.Loader.Load ("image.icon.placeholder.png");
				IconBackground = Fabric.Internal.Editor.View.Render.MakeBackground (200, 200, Fabric.Internal.Editor.View.Render.DBlue);

				IconStyle = new GUIStyle (GUI.skin.button);
				IconStyle.fixedWidth = 192;
				IconStyle.fixedHeight = 192;

				IconBackgroundStyle = new GUIStyle ();
				IconBackgroundStyle.normal.background = IconBackground;
				IconBackgroundStyle.margin = new RectOffset(0, 0, 50, 0);
				IconBackgroundStyle.fixedHeight = 200;
				IconBackgroundStyle.fixedWidth = 200;

				MessageStyle = new GUIStyle ();
				MessageStyle.margin = new RectOffset (20, 20, 10, 0);

				TextStyle = new GUIStyle (GUI.skin.label);
				TextStyle.normal.textColor = Color.white;
				TextStyle.fontSize = 14;
				TextStyle.wordWrap = true;
				TextStyle.padding = new RectOffset (0, 0, 0, 0);
			}

			private static Texture2D LoadIcon(Func<Texture2D> downloadIcon)
			{
				Texture2D[] textures;
				Texture2D texture = null;

				if ((textures = PlayerSettings.GetIconsForTargetGroup (BuildTargetGroup.Android)) != null && textures[0] != null) {
					texture = textures[0];
				}

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
				var buildTargetGroup = BuildTargetGroup.iPhone;
#else
				var buildTargetGroup = BuildTargetGroup.iOS;
#endif
				
				if ((textures = PlayerSettings.GetIconsForTargetGroup (buildTargetGroup)) != null && textures[0] != null) {
					texture = textures[0];
				}

				if (texture == null) {
					texture = downloadIcon ();
				}

				return texture ?? PlaceHolderImage;
			}

			public static void RenderMessage (string message)
			{
				GUILayout.BeginHorizontal (MessageStyle);
				GUILayout.FlexibleSpace ();
				GUILayout.Label (message, TextStyle);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}

			public static void RenderIcon(Func<Texture2D> downloadIcon, Func<string> dashboardUrl)
			{
				Texture2D background = LoadIcon (downloadIcon);

				IconStyle.normal.background = background;
				IconStyle.hover.background = background;
				IconStyle.active.background = background;

				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.BeginVertical (IconBackgroundStyle);

				if (GUILayout.Button ("", IconStyle)) {
					Application.OpenURL (dashboardUrl () + "/issues");
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
			RenderHeader ("We're all done!");
			RenderFooter (back, null);
			Components.RenderMessage ("Click on your app icon to go to the Crashlytics dashboard.");
			Components.RenderIcon (DownloadIcon, DashboardUrl);
		}
	}
}