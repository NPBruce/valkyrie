namespace Fabric.Internal.Editor.API
{
	using Fabric.Internal.Editor.Net.OAuth;
	using Fabric.Internal.Editor.Model;
	using System;
	using System.Collections.Generic;

	internal class AsyncV1
	{
		private static Client client = new Client (Net.Constants.URI);

		public static void Fetch<T>(Action<T> onSuccess, Action<string> onFailure, Func<API.V1, T> fetch)
		{
			Fetch<T> (1, new TimeSpan (0), onSuccess, onFailure, fetch);
		}

		public static void Fetch<T> (Action<T> onSuccess, Action<Exception> onFailure, Action<Exception> onNoNetwork, Func<API.V1, T> fetch)
		{
			Fetch<T> (1, new TimeSpan (0), onSuccess, onFailure, onNoNetwork, fetch);
		}

		public static void Fetch<T>(uint retryCount, TimeSpan retryDelay, Action<T> onSuccess, Action<string> onFailure, Func<API.V1, T> fetch)
		{
			Fetch<T> (retryCount, retryDelay, onSuccess,
				(Exception e) => {
					onFailure ("Request failed: " + e.Message);
				},
				(Exception e) => {
					onFailure ("Network is not available.");
				}, fetch
			);
		}

		public static void Fetch<T>(uint retryCount, TimeSpan retryDelay, Action<T> onSuccess, Action<Exception> onFailure, Action<Exception> onNoNetwork, Func<API.V1, T> fetch)
		{
			Client.Token token = Settings.Instance.Token;

			new Detail.AsyncTaskRunnerBuilder<T> ().Do ((object[] args) => {
				return Net.Validator.MakeRequest (() => {
					return fetch (new API.V1 (
						client.URI,
						client,
						token
					));
				});
			}).OnError ((Exception e) => {
				Exception error = e;

				if (Net.Utils.IsNetworkUnavailableFrom (error)) {
					onNoNetwork (error);
					return Detail.AsyncTaskRunner<T>.ErrorRecovery.Nothing;
				}

				if (error is API.V1.UnauthorizedException) {
					try {
						token = RefreshToken ();
						return Detail.AsyncTaskRunner<T>.ErrorRecovery.Retry;
					} catch (System.Exception refreshException) {
						error = refreshException;
					}
				} else if (error is API.V1.ApiException) {
					return Detail.AsyncTaskRunner<T>.ErrorRecovery.Retry;
				}
				
				onFailure (error);
				return Detail.AsyncTaskRunner<T>.ErrorRecovery.Nothing;
			}).OnCompletion ((T result) => {
				onSuccess (result);
			}).Retry (retryCount, retryDelay).Run ();
		}

		private static Client.Token RefreshToken()
		{
			Settings.Instance.Token = client.Refresh (Settings.Instance.Token);
			return Settings.Instance.Token;
		}
	}
}
