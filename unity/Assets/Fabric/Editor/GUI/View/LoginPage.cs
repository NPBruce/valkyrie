namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using Fabric.Internal.Editor.Model;
	using Fabric.Internal.Editor.Controller;
	using System;
	
	internal class LoginPage : Page
	{
		private string password;
		private Action<string, Action<string>> onLoginButtonClick;
		private Action onSignupLinkClick;
		private string status = "";

		public LoginPage(Action<string, Action<String>> onLoginButtonClick)
		{
			this.onLoginButtonClick = onLoginButtonClick;
			this.onSignupLinkClick = delegate() {
				Application.OpenURL ("https://fabric.io/sign_up");
			};
		}

		#region Components
		private static class Components
		{
			private static readonly GUIStyle LogoStyle;
			private static readonly GUIStyle LogoTextStyle;
			private static readonly GUIStyle LogoSloganStyle;

			private static readonly GUIStyle ErrorStyle;

			private static readonly GUIStyle FieldWrapperStyle;
			private static readonly GUIStyle FieldLabelStyle;
			private static readonly GUIStyle FieldStyle;

			private static readonly GUIStyle PowerButtonStyle;

			private static readonly GUIStyle SignUpStyle;
			private static readonly GUIStyle TextStyle;
			private static readonly GUIStyle LinkStyle;
			
			private static readonly Color32 BorderColor = new Color32 (255, 255, 255, 76);
			private static readonly Color32 ErrorColor = View.Render.FromHex (0xF39C12);
			private static readonly Color32 LinkNormalColor = new Color32 (58, 158, 235, 255);
			
			private static readonly Texture2D Logo = Images.Loader.Load ("fabric-icon.png");
			private static readonly Texture2D LogoText = Images.Loader.Load ("image.logo-text@2x.png");
			private static readonly Texture2D LogoSlogan = Images.Loader.Load ("image.power.on.words.png");
			private static readonly Texture2D PowerButton = Images.Loader.Load ("control.button.power.inactive@2x.png");
			private static readonly Texture2D PowerButtonHover = Images.Loader.Load ("control.button.power.hover@2x.png");
			private static readonly Texture2D PowerButtonClicked = Images.Loader.Load ("control.button.power.active@2x.png");

			private static readonly Texture2D FieldBackground;

			private static readonly GUIContent EmailTextContent;
			private static readonly GUIContent PasswordTextContent;

			static Components()
			{
				EmailTextContent = new GUIContent ();
				PasswordTextContent = new GUIContent ();

				LogoStyle = new GUIStyle ();
				LogoStyle.fixedHeight = 99;
				LogoStyle.fixedWidth = 95;
				LogoStyle.margin = new RectOffset (0, 0, 40, 0);

				LogoTextStyle = new GUIStyle ();
				LogoTextStyle.fixedHeight = 52;
				LogoTextStyle.fixedWidth = 163;
				LogoTextStyle.margin = new RectOffset (0, 0, 10, 0);

				LogoSloganStyle = new GUIStyle ();
				LogoSloganStyle.fixedHeight = 11;
				LogoSloganStyle.fixedWidth = 203;
				LogoSloganStyle.margin = new RectOffset (0, 0, 15, 0);

				ErrorStyle = new GUIStyle (GUI.skin.label);
				ErrorStyle.normal.textColor = ErrorColor;
				ErrorStyle.margin = new RectOffset (0, 0, 5, 5);

				FieldWrapperStyle = new GUIStyle ();
				FieldWrapperStyle.normal.background = View.Render.MakeBackground (1, 1, BorderColor);
				FieldWrapperStyle.margin = new RectOffset (20, 20, 10, 0);

				FieldLabelStyle = new GUIStyle (GUI.skin.label);
				FieldLabelStyle.normal.textColor = Color.white;
				FieldLabelStyle.active.textColor = Color.white;
				FieldLabelStyle.fontStyle = FontStyle.Bold;
				FieldLabelStyle.margin = new RectOffset (2, 2, 2, 2);

				FieldBackground = View.Render.MakeBackground (1, 1, View.Render.LBlue);
				FieldStyle = new GUIStyle (GUI.skin.textField);
				FieldStyle.fontSize = 15;
				FieldStyle.padding = new RectOffset(2, 2, 8, 8);
				FieldStyle.wordWrap = false;
				FieldStyle.normal.background = FieldBackground;
				FieldStyle.active.background = FieldBackground;
				FieldStyle.focused.background = FieldBackground;
				FieldStyle.active.textColor = Color.white;
				FieldStyle.normal.textColor = Color.white;
				FieldStyle.focused.textColor = Color.white;
				FieldStyle.margin = new RectOffset (2, 2, 0, 2);

				PowerButtonStyle = new GUIStyle (GUI.skin.button);
				PowerButtonStyle.fixedWidth = 75;
				PowerButtonStyle.fixedHeight = 72;
				PowerButtonStyle.normal.background = PowerButton;
				PowerButtonStyle.hover.background = PowerButtonHover;
				PowerButtonStyle.active.background = PowerButtonClicked;
				PowerButtonStyle.margin = new RectOffset (0, 0, 25, 0);

				SignUpStyle = new GUIStyle ();
				SignUpStyle.margin = new RectOffset (20, 20, 10, 0);

				TextStyle = new GUIStyle (GUI.skin.label);
				TextStyle.normal.textColor = Color.white;
				TextStyle.wordWrap = true;
				TextStyle.padding = new RectOffset (0, 0, 0, 0);

				LinkStyle = new GUIStyle (GUI.skin.label);
				LinkStyle.normal.textColor = LinkNormalColor;
				LinkStyle.padding = new RectOffset (0, 0, 0, 0);
			}

			private static void HandleKeyEvents(string input, Action onChange, Action onEnter)
			{
				Event current = Event.current;

				if (current.type == EventType.KeyDown) {
					if (current.keyCode == KeyCode.Return && input.Length != 0) {
						onEnter ();
						return;
					}
					onChange ();
				}
			}

			public static void RenderEmailField(string bound, Action<string> set, Action onChange, Action onEnter)
			{
				GUILayout.BeginVertical (FieldWrapperStyle);
				GUILayout.Label ("Email", FieldLabelStyle);

				HandleKeyEvents (bound, onChange, onEnter);

				Color old = GUI.skin.settings.cursorColor;
				GUI.skin.settings.cursorColor = Color.white;

				EmailTextContent.text = bound;
				Rect fieldRect = GUILayoutUtility.GetRect (EmailTextContent, FieldStyle);
				set (EditorGUI.TextField (fieldRect, bound ?? "", FieldStyle));

				GUI.skin.settings.cursorColor = old;

				GUILayout.EndVertical ();
			}
			
			public static void RenderPasswordField(string bound, Action<string> set, Action onChange, Action onEnter)
			{
				GUILayout.BeginVertical (FieldWrapperStyle);
				GUILayout.Label ("Password", FieldLabelStyle);

				bound = bound == null ? "" : bound;

				HandleKeyEvents (bound, onChange, onEnter);

				Color old = GUI.skin.settings.cursorColor;
				GUI.skin.settings.cursorColor = Color.white;

				PasswordTextContent.text = string.Empty.PadRight (bound.Length, '*');
				Rect fieldRect = GUILayoutUtility.GetRect (PasswordTextContent, FieldStyle);
				set (EditorGUI.PasswordField (fieldRect, bound, FieldStyle));

				GUI.skin.settings.cursorColor = old;

				GUILayout.EndVertical ();
			}
			
			public static void RenderLoginButton(Action onClick)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				
				if (GUILayout.Button ("", PowerButtonStyle)) {
					onClick();
				}
				
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}

			public static void RenderLogo()
			{
				GUILayout.BeginVertical ();
				CenterLabel (Logo, LogoStyle);
				CenterLabel (LogoText, LogoTextStyle);
				CenterLabel (LogoSlogan, LogoSloganStyle);
				GUILayout.EndVertical ();
			}

			private static void CenterLabel (Texture2D content, GUIStyle style)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label (content, style);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}

			public static void RenderLoginStatus(string message)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label (message, ErrorStyle);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}

			public static void RenderSignUp(Action linkAction)
			{
				GUILayout.BeginHorizontal (SignUpStyle);
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("New to Fabric?", TextStyle);
				if (GUILayout.Button ("Sign up now!", LinkStyle)) {
					linkAction ();
				}
				EditorGUIUtility.AddCursorRect (GUILayoutUtility.GetLastRect (), MouseCursor.Link);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}
		}
		#endregion

		public override void RenderImpl(Rect position)
		{
			Action loginButtonClick = delegate() {
				onLoginButtonClick (password, message => status = message);
			};

			Components.RenderLogo ();
			Components.RenderLoginStatus (status);
			Components.RenderEmailField (Settings.Instance.Email, value => Settings.Instance.Email = value, () => status = "", loginButtonClick);
			Components.RenderPasswordField (password, value => password = value, () => status = "", loginButtonClick);
			Components.RenderSignUp (onSignupLinkClick);
			Components.RenderLoginButton (loginButtonClick);
		}
	}
}
