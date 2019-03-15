namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	
	internal class UpdatePage : Page
	{
		public delegate void AcceptUpdateBehavior (
			Action<float> onProgress,
			Action<string> downloadComplete,
			Action<System.Exception> downloadError,
			Action verificationError,
			Func<bool> isCancelled
		);

		private readonly AcceptUpdateBehavior onAcceptUpdate;
		private readonly KeyValuePair<string, Action> back;
		private readonly KeyValuePair<string, Action> install;
		private readonly KeyValuePair<string, Action> retry;
		private readonly KeyValuePair<string, Action> cancel;

		private enum Status {
			Idle,
			InProgress,
			Failed,
			Cancelled,
			Completed,
			NetworkUnavailable,
			VerificationFailed
		}

		private volatile Status status;
		private float downloadProgress;
		private Func<string> getDisplayTitle;
		private Func<string> fetchReleaseNotes;

		public UpdatePage(AcceptUpdateBehavior onAcceptUpdate, Action onBack, Func<string> getDisplayTitle, Func<string> fetchReleaseNotes)
		{
			this.onAcceptUpdate = onAcceptUpdate;
			this.back = new KeyValuePair<string, Action> ("Back", Reset + onBack);
			this.install = new KeyValuePair<string, Action> ("Install", InstallUpdate);
			this.retry = new KeyValuePair<string, Action> ("Retry", InstallUpdate);
			this.cancel = new KeyValuePair<string, Action> ("Cancel", Cancel);
			this.getDisplayTitle = getDisplayTitle;
			this.fetchReleaseNotes = fetchReleaseNotes;
		}

		#region Components
		private static class Components
		{
			private static readonly GUIStyle ReleaseNotesStyle;
			private static readonly GUIStyle DownloadFailedPaneStyle;
			private static readonly GUIStyle DownloadFailedLabelStyle;
			private static readonly GUIStyle ProgessPaneStyle;
			private static readonly GUIStyle ProgressLabelStyle;
			private static readonly GUIStyle ProgressBarStyle;
			private static readonly GUIStyle ProgressBarFilledPartStyle;
			private static readonly GUIStyle ProgressBarUnfilledPartStyle;
			private static readonly GUIStyle ImportLabelStyle;
			private static readonly GUIStyle ScrollStyle;

			private static Vector2 scrollPosition;

			static Components()
			{
				ReleaseNotesStyle = new GUIStyle (GUI.skin.label);
				DownloadFailedPaneStyle = new GUIStyle ();
				DownloadFailedLabelStyle = new GUIStyle (GUI.skin.label);
				ProgessPaneStyle = new GUIStyle ();
				ProgressLabelStyle = new GUIStyle ();
				ProgressBarStyle = new GUIStyle ();
				ProgressBarFilledPartStyle = new GUIStyle ();
				ProgressBarUnfilledPartStyle = new GUIStyle ();
				ImportLabelStyle = new GUIStyle ();
				ScrollStyle = new GUIStyle (GUI.skin.scrollView);

				ReleaseNotesStyle.normal.textColor = Color.white;
				ReleaseNotesStyle.fontSize = 14;
				ReleaseNotesStyle.wordWrap = true;

				ProgessPaneStyle.padding = new RectOffset (18, 18, 18, 18);

				ProgressLabelStyle.normal.textColor = Color.white;
				ProgressLabelStyle.fontSize = 14;
				ProgressLabelStyle.margin = new RectOffset (20, 20, 0, 10);

				ImportLabelStyle.normal.textColor = Color.white;
				ImportLabelStyle.margin = new RectOffset (20, 20, 10, 10);
				ImportLabelStyle.wordWrap = true;

				ProgressBarFilledPartStyle.normal.background = View.Render.MakeBackground (1, 1, new Color32 (22,82,129,255));
				ProgressBarUnfilledPartStyle.normal.background = View.Render.MakeBackground (1, 1, new Color32 (16, 58, 90, 255));

				DownloadFailedPaneStyle.padding = new RectOffset (18, 18, 18, 18);
				DownloadFailedPaneStyle.margin.bottom = 75;

				DownloadFailedLabelStyle.normal.textColor = new Color32 (211, 152, 29, 255);
				DownloadFailedLabelStyle.fontSize = 13;
				DownloadFailedLabelStyle.fontStyle = FontStyle.Bold;
				DownloadFailedLabelStyle.wordWrap = true;
				DownloadFailedLabelStyle.alignment = TextAnchor.MiddleCenter;

				ScrollStyle.margin.top = 18;
				ScrollStyle.margin.bottom = 75;
				ScrollStyle.padding = new RectOffset (18, 18, 0, 0);
			}

			public static void RenderReleaseNotes(string notes)
			{
				scrollPosition = GUILayout.BeginScrollView (scrollPosition, ScrollStyle);
				GUILayout.Label (notes, ReleaseNotesStyle);
				GUILayout.EndScrollView ();
			}

			public static void RenderDownloadBar(Rect position, float progress, string downloadLabel, string importLabel)
			{
				GUILayout.BeginVertical (ProgessPaneStyle);
				GUILayout.Label (downloadLabel, ProgressLabelStyle);
				GUILayout.BeginHorizontal (ProgressBarStyle);
				float progressWidth = progress * (position.width - ProgessPaneStyle.padding.horizontal - ProgessPaneStyle.margin.horizontal);
				GUILayout.Box (GUIContent.none, ProgressBarFilledPartStyle, GUILayout.Width (progressWidth));
				GUILayout.Box (GUIContent.none, ProgressBarUnfilledPartStyle, GUILayout.ExpandWidth (true));
				GUILayout.EndHorizontal ();
				GUILayout.Label (importLabel, ImportLabelStyle);
				GUILayout.EndVertical ();
			}

			public static void RenderDownloadFailed()
			{
				GUILayout.BeginVertical (DownloadFailedPaneStyle);
				GUILayout.Label ("Oops, looks like something odd happened.", DownloadFailedLabelStyle);
				GUILayout.Label ("Your download has failed!", DownloadFailedLabelStyle);
				GUILayout.Label ("You can try again by clicking below, or contact support@fabric.io.", DownloadFailedLabelStyle);
				GUILayout.EndVertical ();
			}

			public static void RenderNetworkUnavailable()
			{
				GUILayout.BeginVertical (DownloadFailedPaneStyle);
				GUILayout.Label (
					"Looks like a valid network connection is not available. Please re-connect to the internet and try again.",
					DownloadFailedLabelStyle
				);
				GUILayout.EndVertical ();
			}

			public static void RenderVerificationFailed()
			{
				GUILayout.BeginVertical (DownloadFailedPaneStyle);
				GUILayout.Label ("Oops, looks like something odd happened.", DownloadFailedLabelStyle);
				GUILayout.Label ("We couldn't verify the downloaded binaries!", DownloadFailedLabelStyle);
				GUILayout.Label ("Please contact support@fabric.io.", DownloadFailedLabelStyle);
				GUILayout.EndVertical ();
			}
		}
		#endregion

		public override void RenderImpl(Rect position)
		{
			RenderHeader (getDisplayTitle ());

			KeyValuePair<string, Action>? next = null;
			string downloadLabel = getStatusLabel (status);
			string importLabel = "";

			switch (status) {
			case Status.InProgress:
				Components.RenderDownloadBar (position, downloadProgress, downloadLabel, importLabel);
				next = cancel;
				break;
			case Status.Completed:
				importLabel = "This may take a few moments.";
				Components.RenderDownloadBar (position, downloadProgress, downloadLabel, importLabel);
				break;
			case Status.Failed:
				Components.RenderDownloadBar (position, downloadProgress, downloadLabel, importLabel);
				Components.RenderDownloadFailed ();
				next = retry;
				break;
			case Status.NetworkUnavailable:
				Components.RenderDownloadBar (position, downloadProgress, downloadLabel, importLabel);
				Components.RenderNetworkUnavailable ();
				next = retry;
				break;
			case Status.Cancelled:
				Components.RenderDownloadBar (position, downloadProgress, downloadLabel, importLabel);
				next = retry;
				break;
			case Status.VerificationFailed:
				Components.RenderDownloadBar (position, downloadProgress, downloadLabel, importLabel);
				Components.RenderVerificationFailed ();
				next = retry;
				break;
			case Status.Idle:
			default:
				Components.RenderReleaseNotes (fetchReleaseNotes ());
				next = install;
				break;
			}

			RenderFooter (back, next);
		}

		private void InstallUpdate()
		{
			downloadProgress = 0f;
			status = Status.InProgress;
			onAcceptUpdate (
				HandleProgressUpdate,
				HandleDownloadComplete,
				HandleDownloadError,
				HandleVerificationError,
				CheckIsCancelled
			);
		}

		private void Reset()
		{
			downloadProgress = 0f;
			status = Status.Idle;
		}

		private void Cancel()
		{
			status = Status.Cancelled;
		}

		private bool CheckIsCancelled()
		{
			return status == Status.Cancelled || status == Status.Idle;
		}
		
		private void HandleProgressUpdate(float progress)
		{
			this.downloadProgress = progress;
		}

		private void HandleDownloadComplete(string downloadPath)
		{
			status = Status.Completed;
		}

		private void HandleDownloadError(System.Exception exception)
		{
			status = Net.Utils.IsNetworkUnavailableFrom (exception) ?
				Status.NetworkUnavailable :
				Status.Failed;
		}

		private void HandleVerificationError()
		{
			status = Status.VerificationFailed;
		}

		private static string getStatusLabel(Status status)
		{
			switch (status) {
			case Status.Completed:
				return "Waiting for import...";
			case Status.Cancelled:
				return "Cancelled...";
			case Status.InProgress:
			default:
				return "Downloading...";
			}
		}
	}
}
