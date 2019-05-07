namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using Fabric.Internal.Editor.Controller;
	using Fabric.Internal.Editor.Model;
	using KitsList = System.Collections.Generic.List<Update.Dependency.DependencyGraphObject.DependenciesObject.KitsObject>;
	using KitsObject = Update.Dependency.DependencyGraphObject.DependenciesObject.KitsObject;
	
	internal class KitSelectionPage : Page
	{
		private const string ButtonTextFormatString = "<size=10><color={0}>{1}</color></size>";

		public delegate bool CheckUpdateAvailable ();

		private readonly CheckUpdateAvailable isUpdateAvailable;

		private Func<List<ImportedKit>> listImportedKits;
		private Func<KitsList> listAvailableKits;
		private Action checkKitActivation;

		private Action<KitsObject, ImportedKit, bool> onKitSelected;
		private KeyValuePair<string, Action> back;
		private KeyValuePair<string, Action> viewUpdateClickHandler;
		
		private KitsList availableKits;
		private List<ImportedKit> importedKits;

		public KitSelectionPage(
			Func<KitsList> listAvailableKits,
			Func<List<ImportedKit>> listImportedKits,
			Action<KitsObject, ImportedKit, bool> onKitSelected,
			Action checkKitActivation,
			Action onBackClicked,
			CheckUpdateAvailable isUpdateAvailable,
			Action showUpdatePage
		)
		{
			this.listImportedKits = listImportedKits;
			this.listAvailableKits = listAvailableKits;
			this.onKitSelected = onKitSelected;
			this.checkKitActivation = checkKitActivation;
			this.back = new KeyValuePair<string, Action> ("Back", onBackClicked);
			this.viewUpdateClickHandler = new KeyValuePair<string, Action> ("View Update", showUpdatePage);
			this.isUpdateAvailable = isUpdateAvailable;
		}
		
		#region Components
		private static class Components
		{
			private static readonly GUIStyle RowStyle;
			private static readonly GUIStyle ScrollStyle;
			
			private static readonly Color32 SelectedColor = View.Render.LBlue;
			private static readonly Color32 RowColor = View.Render.FromHex (0x2B6591);
			
			private static Vector2 scroll;
			
			private static readonly int padding = 14;
			
			static Components()
			{
				RowStyle = new GUIStyle (GUI.skin.button);
				ScrollStyle = new GUIStyle (GUI.skin.scrollView);

				RowStyle.padding = new RectOffset (padding, padding, padding, padding);
				RowStyle.alignment = TextAnchor.MiddleLeft;
				RowStyle.fontSize = 14;
				RowStyle.normal.textColor = Color.white;
				RowStyle.richText = true;
				
				int rowHeight = RowStyle.normal.background.height;
				int rowWidth = RowStyle.normal.background.width;
				
				RowStyle.normal.background = View.Render.MakeBackground (rowWidth, rowHeight, RowColor);
				RowStyle.hover.background = View.Render.MakeBackground (rowWidth, rowHeight, SelectedColor);
				
				ScrollStyle.margin.top = 18;
				ScrollStyle.margin.bottom = 18 + 75;
				ScrollStyle.margin.left = 18;
				ScrollStyle.margin.right = 16;
			}

			private static string Stylize(string raw, string color = "silver")
			{
				return String.Format (ButtonTextFormatString, color, raw);
			}

			private static string Separate(string name, string status)
			{
				return name + "\n" + status;
			}

			private static string MakeRowCaption(KitRowData rowData)
			{
				if (KitStatus.Available != rowData.Status && !rowData.IsUpToDate) {
					string updateText = String.Format ("Update available: v{0}", rowData.LatestVersion);
					return Separate (rowData.Name, Stylize (updateText, "orange"));
				}

				string stateString = String.Empty;
				switch (rowData.Status) {
				case KitStatus.Installed:
					stateString = String.Format ("v{0} Installed", rowData.CurrentVersion);
					break;
				case KitStatus.Configured:
					stateString = String.Format ("v{0} Configured - waiting for app launch", rowData.CurrentVersion);
					break;
				case KitStatus.Imported:
					stateString = String.Format ("Click to configure v{0}", rowData.CurrentVersion);
					break;
				case KitStatus.Available:
					stateString = String.Format ("Click to install v{0}", rowData.LatestVersion);
					break;
				}

				return Separate (rowData.Name, Stylize (stateString));
			}
			
			public static void RenderKitList(
				List<KitRowData> kitRowDataList,
				Action<KitsObject, ImportedKit, bool> onSelected,
				Action clearKitLists,
				bool disabled
			)
			{
				scroll = GUILayout.BeginScrollView (scroll, ScrollStyle);

				GUI.enabled = !disabled;
				foreach (KitRowData rowData in kitRowDataList) {
					if (GUILayout.Button (MakeRowCaption(rowData), RowStyle)) {
						onSelected (rowData.AvailableKit, rowData.ImportedKit, true);
						clearKitLists ();
					}
				}
				GUI.enabled = true;
				
				GUILayout.EndScrollView ();
			}
		}
		#endregion
		
		public override void RenderImpl(Rect position)
		{
			if (isUpdateAvailable ()) {
				RenderBanner ("Fabric plugin update available", viewUpdateClickHandler);
			}

			RenderHeader ("Please select a kit to install");

			importedKits = importedKits == null ? listImportedKits () : importedKits;
			availableKits = listAvailableKits ();

			if (availableKits == null) {
				return;
			}

			List<KitRowData> kitRowDataList = new List<KitRowData> ();

			bool hasConfiguredKits = false;
			bool hasInstalledKits = false;

			foreach (var availableKit in availableKits) {
				ImportedKit importedKit = (importedKits == null) ? null : importedKits.Find ((ImportedKit k) => {
					return k.Name.Equals (availableKit.Name, StringComparison.OrdinalIgnoreCase);
				});

				KitRowData kitRowData = new KitRowData (availableKit, importedKit);

				hasConfiguredKits |= kitRowData.Status == KitStatus.Configured;
				hasInstalledKits |= kitRowData.Status == KitStatus.Installed;

				kitRowDataList.Add (kitRowData);
			}

			if (hasConfiguredKits) {
				checkKitActivation ();
			}

			bool shouldDisableBack = EditorApplication.isCompiling || hasConfiguredKits || hasInstalledKits;
			if (shouldDisableBack) {
				RenderFooter (null, null);
			} else {
				RenderFooter (back, null);
			}
			
			Components.RenderKitList (
				kitRowDataList,
				onKitSelected,
				clearKitLists,
				EditorApplication.isCompiling
			);
		}

		private void clearKitLists()
		{
			importedKits = null;
			availableKits = null;
		}

		private class KitRowData
		{
			public KitsObject AvailableKit;
			public ImportedKit ImportedKit;

			public string Name;
			public bool IsUpToDate;
			public KitStatus Status;
			public string CurrentVersion;
			public string LatestVersion;

			public KitRowData(KitsObject availableKit, ImportedKit importedKit)
			{
				this.AvailableKit = availableKit;
				this.ImportedKit = importedKit;

				this.LatestVersion = availableKit.Version.ToString ();
				this.IsUpToDate = KitUtils.IsUpToDate(availableKit, importedKit);

				this.Name = availableKit.Name;
				this.Status = KitStatus.Available;
				this.CurrentVersion = new System.Version ().ToString (); // Default is 0.0

				if (importedKit != null) {
					this.Name = importedKit.Name;
					this.Status = KitUtils.StatusFor (importedKit);
					this.CurrentVersion = importedKit.Instance.Version ().ToString ();
				}
			}
		}
	}
}