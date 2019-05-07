namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	
	public abstract class Page
	{
		private readonly GUIStyle BannerStyle;
		private readonly GUIStyle BannerTextStyle;
		private readonly GUIStyle HeaderStyle;
		private readonly GUIStyle HeaderLineStyle;
		private readonly GUIStyle FooterLineStyle;
		private readonly GUIStyle RButtonStyle;
		private readonly GUIStyle LButtonStyle;

		private static Rect FooterPosition = new Rect ();
		private static Rect LButtonPosition = new Rect ();
		private static Rect RButtonPosition = new Rect ();

		public Page()
		{
			BannerStyle = new GUIStyle ();
			BannerTextStyle = new GUIStyle (GUI.skin.label);
			HeaderStyle = new GUIStyle ();
			HeaderLineStyle = new GUIStyle ();
			FooterLineStyle = new GUIStyle ();
			RButtonStyle = new GUIStyle (GUI.skin.button);
			LButtonStyle = new GUIStyle (GUI.skin.button);

			BannerStyle.normal.background = View.Render.MakeBackground (1, 1, new Color32(206, 90, 53, 255));
			BannerStyle.padding = new RectOffset (10, 10, 10, 10);

			BannerTextStyle.fontSize = 12;
			BannerTextStyle.normal.textColor = Color.white;

			HeaderStyle.normal.textColor = Color.white;
			HeaderStyle.fontSize = 15;
			HeaderStyle.fontStyle = FontStyle.Bold;
			HeaderStyle.margin.left = 20;
			HeaderStyle.margin.top = 20;
			HeaderStyle.margin.bottom = 20;
			HeaderStyle.wordWrap = true;

			HeaderLineStyle.fixedHeight = 1;
			HeaderLineStyle.normal.background = View.Render.MakeBackground (1, 1, new Color32 (255, 255, 255, 76));

			FooterLineStyle.fixedHeight = 1;
			FooterLineStyle.normal.background = View.Render.MakeBackground (1, 1, new Color32 (255, 255, 255, 76));

			LButtonStyle.fixedHeight = 29;
			LButtonStyle.fixedWidth = 52;
			LButtonStyle.margin.left = 20;
			LButtonStyle.margin.top = 25;
			LButtonStyle.margin.bottom = 20;
			
			RButtonStyle.fixedHeight = 29;
			RButtonStyle.fixedWidth = 52;
			RButtonStyle.margin.left = 5;
			RButtonStyle.margin.top = 25;
			RButtonStyle.margin.bottom = 20;
			RButtonStyle.margin.right = 20;
		}

		public void RenderBanner(string message, KeyValuePair<string, Action>? button)
		{
			GUILayout.BeginHorizontal (BannerStyle);
			GUILayout.Label (message);
			GUILayout.FlexibleSpace ();
			if (button.HasValue && GUILayout.Button (button.Value.Key, GUI.skin.button)) {
				button.Value.Value ();
			}
			GUILayout.EndHorizontal ();
		}

		public void RenderHeader(string message)
		{
			EditorGUILayout.Space ();
			GUILayout.Label (message, HeaderStyle);
			GUILayout.Label ("", HeaderLineStyle);
			EditorGUILayout.Space ();
		}

		public void RenderFooter (KeyValuePair<string, Action>? lButton, KeyValuePair<string, Action>? rButton)
		{
			GUI.Label (FooterPosition, "", FooterLineStyle);

			GUILayout.BeginHorizontal ();

			if (lButton.HasValue) {
				RenderButton (lButton.Value.Key, lButton.Value.Value, LButtonStyle, LButtonPosition);
			}
			GUILayout.FlexibleSpace ();

			if (rButton.HasValue) {
				RenderButton (rButton.Value.Key, rButton.Value.Value, RButtonStyle, RButtonPosition);
			}
			
			GUILayout.EndHorizontal ();
		}
		
		private void RenderButton(string caption, Action onClick, GUIStyle style, Rect position)
		{
			if (GUI.Button (position, caption, style)) {
				onClick ();
			}
		}

		private void Reposition(Rect position)
		{
			FooterPosition.width = position.width;
			FooterPosition.height = 30;
			FooterPosition.x = 0;
			FooterPosition.y = position.height - (HeaderStyle.margin.top + HeaderStyle.margin.bottom + 15 + 13);

			LButtonPosition.width = 52;
			LButtonPosition.height = 29;
			LButtonPosition.x = 20;
			LButtonPosition.y = position.height - (29 + 20);

			RButtonPosition.width = 52;
			RButtonPosition.height = 29;
			RButtonPosition.x = position.width - (20 + 52);
			RButtonPosition.y = position.height - (29 + 20);
		}

		public void Render(Rect position)
		{
			Reposition (position);
			RenderImpl (position);
		}

		public abstract void RenderImpl(Rect position);
	}
}