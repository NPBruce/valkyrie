namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.Net;
	using Fabric.Internal.Editor.Model;
	using System;
	
	internal class OrganizationsPage : Page
	{	
		private Action<Organization> onOrganizationSelected;
		private Action asyncFetchOrganizations;
		private List<Organization> organizations = null;
		private string error = null;
		private volatile bool started = false;

		private KeyValuePair<string, Action> retry;

		public OrganizationsPage(Action<Organization> onOrganizationSelected, Action<Action<List<Organization>>, Action<string>> asyncFetchOrganizations)
		{
			this.onOrganizationSelected = onOrganizationSelected;
			this.asyncFetchOrganizations = () => {
				if (started) {
					return;
				}

				started = true;
				asyncFetchOrganizations (SetOrganizations, SetError);
			};
			this.retry = new KeyValuePair<string, Action> ("Retry", delegate () {
				this.started = false;
				this.asyncFetchOrganizations ();
			});
		}

		#region Components
		private static class Components
		{
			private static readonly GUIStyle ErrorStyle;
			private static readonly GUIStyle RowStyle;
			private static readonly GUIStyle ScrollStyle;
			private static readonly GUIStyle LabelStyle;

			private static readonly Color32 SelectedColor = View.Render.LBlue;
			private static readonly Color32 RowColor = View.Render.FromHex (0x2B6591);
			private static readonly Color32 ErrorColor = View.Render.FromHex (0xF39C12);

			private static Vector2 scroll;

			private static readonly int padding = 16;

			static Components()
			{
				LabelStyle = new GUIStyle (GUI.skin.label);
				LabelStyle.normal.textColor = Color.white;
				LabelStyle.padding.top = 10;
				LabelStyle.padding.left = 20;
				LabelStyle.padding.right = 20;
				LabelStyle.wordWrap = true;

				RowStyle = new GUIStyle (GUI.skin.button);
				RowStyle.padding = new RectOffset (padding, padding, padding, padding);
				RowStyle.alignment = TextAnchor.MiddleLeft;
				RowStyle.fontSize = 14;
				RowStyle.normal.textColor = Color.white;

				ErrorStyle = new GUIStyle (LabelStyle);
				ErrorStyle.normal.textColor = ErrorColor;

				int rowHeight = RowStyle.normal.background.height;
				int rowWidth = RowStyle.normal.background.width;

				RowStyle.normal.background = View.Render.MakeBackground (rowWidth, rowHeight, RowColor);
				RowStyle.hover.background = View.Render.MakeBackground (rowWidth, rowHeight, SelectedColor);

				ScrollStyle = new GUIStyle (GUI.skin.scrollView);
				ScrollStyle.margin.top = 18;
				ScrollStyle.margin.bottom = 0;
				ScrollStyle.margin.left = 18;
				ScrollStyle.margin.right = 16;
			}

			public static void RenderOrganizationsList(ICollection<Organization> organizations, Action<Organization> onSelected)
			{
				scroll = GUILayout.BeginScrollView (scroll, ScrollStyle);

				for (int i = 0; i < 1; ++i) {
					foreach (Organization organization in organizations) {
						if (GUILayout.Button (organization.Name, RowStyle)) {
							onSelected (organization);
						}
					}
				}

				GUILayout.EndScrollView ();
			}

			public static void RenderError(string error)
			{
				CenterHorizontalLabel (
					"An error occured while trying to fetch the Organizations list associated with your account: " + error,
					ErrorStyle
				);
			}

			public static void RenderFetching()
			{
				CenterHorizontalLabel ("Fetching organizations...", LabelStyle);
			}

			private static void CenterHorizontalLabel(string text, GUIStyle labelStyle)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label (text, labelStyle);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}
		}
		#endregion

		private void SetOrganizations(List<Organization> organizations)
		{
			this.organizations = organizations;
			this.error = null;
		}

		private void SetError(string error)
		{
			this.error = error;
			this.organizations = null;
		}

		public override void RenderImpl(Rect position)
		{
			RenderHeader ("Please select your organization");

			asyncFetchOrganizations ();

			if (error != null) {
				Components.RenderError (error);
				RenderFooter (null, retry);
			} else if (organizations != null) {
				// Unity has two related events; Layout and Repaint. When the content of the GUI changes
				// _between_ those two events, Unity will throw an error in the format of:
				//    "ArgumentException: Getting control X's position in a group with only X controls when doing Repaint"
				// Ideally, no changes should be made in OnGUI (). At some time in the future, we should rewrite this plugin
				// in terms of Update (). Until then, we can get around these errors by changing the gui only on the Repaint
				// event.
				if (organizations.Count == 1 && Event.current.type == EventType.Repaint) {
					onOrganizationSelected (organizations[0]);
					return;
				}
				Components.RenderOrganizationsList (organizations, onOrganizationSelected);
			} else {
				Components.RenderFetching ();
			}
		}
	}
}
