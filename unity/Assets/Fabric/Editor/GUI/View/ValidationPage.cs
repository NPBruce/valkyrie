namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Collections;
	using System;
	using Fabric.Internal.Editor.Controller;
	
	internal class ValidationPage : Page
	{
		private static readonly Texture2D Rocket = Images.Loader.Load ("image.rocket.png");
		private KeyValuePair<string, Action> finishButton;

		public ValidationPage(Action finish, Action setKitConfigured)
		{
			finishButton = new KeyValuePair<string, Action> ("Finish", delegate() {
				setKitConfigured ();
				finish ();
			});
		}

		#region Components
		private static class Components
		{
			private static readonly GUIStyle MessageStyle;
			private static readonly GUIStyle TextStyle;
			private static readonly GUIStyle ErrorTextStyle;

			private static readonly GUIStyle RocketStyle;

			private static readonly Color ErrorColor;
			
			static Components()
			{
				ErrorColor = View.Render.FromHex (0xF39C12);

				MessageStyle = new GUIStyle ();
				MessageStyle.margin = new RectOffset (20, 20, 10, 0);

				TextStyle = new GUIStyle (GUI.skin.label);
				TextStyle.normal.textColor = Color.white;
				TextStyle.fontSize = 14;
				TextStyle.wordWrap = true;
				TextStyle.padding = new RectOffset (0, 0, 0, 0);

				ErrorTextStyle = new GUIStyle (TextStyle);
				ErrorTextStyle.normal.textColor = ErrorColor;

				RocketStyle = new GUIStyle ();
				RocketStyle.fixedWidth = 243;
				RocketStyle.margin = new RectOffset (0, 0, 50, 25);
			}

			public static void RenderMessage(string message)
			{
				GUILayout.BeginHorizontal (MessageStyle);
				GUILayout.Label (message, TextStyle);
				GUILayout.EndHorizontal ();
			}

			public static void RenderRocket()
			{
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label (Rocket, RocketStyle);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}
		}
		#endregion

		public override void RenderImpl(Rect position)
		{
			RenderHeader ("Configuration Complete!");
			Components.RenderMessage ("You've successfully configured your app!");
			Components.RenderRocket ();
			Components.RenderMessage ("Build and launch your app on a device to complete the setup process, then view your app on the Fabric dashboard.");
			RenderFooter (null, finishButton);
		}
	}
}
