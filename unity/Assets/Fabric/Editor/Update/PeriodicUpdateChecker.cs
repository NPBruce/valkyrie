namespace Fabric.Internal.Editor.Update
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Threading;
	using Dependency;
	using Fabric.Internal.Editor.Model;
	using KitsList = System.Collections.Generic.List<Dependency.DependencyGraphObject.DependenciesObject.KitsObject>;

	internal class PeriodicUpdateChecker
	{
		private static readonly string dependencyGraphUrl = "https://fabric.io/unity-fabric/latest/dependency-graph.json";

		private readonly long checkPeriodMillis;
		private readonly long delayMillis;
		private readonly TimeoutWebClient client;

		public delegate void UpdateCheckCallback();

		private Timer timer;
		private DependencyGraphManager dependencyGraphManager = new DependencyGraphManager ();
		private UpdateCheckCallback updateCheckCallback = delegate () {};

		public PeriodicUpdateChecker(long checkPeriodMillis, long delayMillis)
		{
			this.checkPeriodMillis = checkPeriodMillis;
			this.delayMillis = delayMillis;
			this.client = new TimeoutWebClient ((int)(checkPeriodMillis * 0.8));
		}

		public void RegisterUpdateCheckCallback(UpdateCheckCallback updateCheckCallback)
		{
			this.updateCheckCallback = updateCheckCallback;
		}

		public void Start()
		{
			if (timer != null || UnityEditorInternal.InternalEditorUtility.inBatchMode) {
				return;
			}

			timer = new Timer (CheckForUpdates, null, delayMillis, checkPeriodMillis);
		}

		public void Stop() 
		{
			if (timer == null) {
				return; // Already stopped.
			}

			timer.Dispose ();
			timer = null;
		}

		public DependencyGraphManager GetDependencyGraphManager()
		{
			return dependencyGraphManager;
		}

		// Runs on a background thread.
		private void CheckForUpdates(object state)
		{
			try {
				bool noNetwork = false;
				string raw = Net.Validator.MakeRequest(
					() => { return client.DownloadString (dependencyGraphUrl); },
					(Exception e) => {
						if (Net.Utils.IsNetworkUnavailableFrom (e)) {
							noNetwork = true;
							return;
						}

						Utils.Warn ("An error occured when trying to fetch the dependency graph; {0}", e.Message);
					}
				);

				if (noNetwork) {
					return;
				}

				if (String.IsNullOrEmpty (raw)) {
					Utils.Warn ("Dependency graph is empty, skipping update check...");
					return;
				}

				if (dependencyGraphManager.Manage (raw)) {
					updateCheckCallback ();
				}
			} catch (System.Net.WebException e) {
				Utils.Warn ("Plugin update check error: {0}", e.Message);
			}
		}
	}
}
