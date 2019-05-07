namespace Fabric.Internal.Editor.Detail
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Threading;

	internal class AsyncTaskRunner<R>
	{
		public enum ErrorRecovery {
			Retry,
			Nothing
		}

		private class RetryFailureException : System.Exception
		{
			public RetryFailureException(string message) : base (message) {
			}
		}

		public delegate R DoInBackground(params object[] input);
		public delegate void OnWorkCompleted(R result);
		public delegate ErrorRecovery OnError(System.Exception e);

		private readonly DoInBackground doInBackground;
		private readonly OnWorkCompleted onWorkCompleted;
		private readonly OnError onError;

		private enum Status { Idle, Running, Error, Done, Stopped };

		private volatile int _status;
		private Status status
		{
			get { return (Status)_status; }
			set { _status = (int)value; }
		}

		private R result;
		private Thread thread;
		private System.Exception exception = null;

		private uint retryCountRemaining;
		private uint retryCount;
		private System.TimeSpan retryDelay;

		private IEnumerator coroutine;

		// Run a single task only once.
		public AsyncTaskRunner (DoInBackground doInBackground, OnWorkCompleted onWorkCompleted, OnError onError) {
			this.status = Status.Idle;
			this.doInBackground = doInBackground;
			this.onWorkCompleted = onWorkCompleted;
			this.onError = onError;
			this.retryCountRemaining = 0;
			this.retryCount = 1;
			this.retryDelay = new System.TimeSpan(0, 0, 0);
		}

		public AsyncTaskRunner(uint retryCount, System.TimeSpan retryDelay, DoInBackground doInBackground, OnWorkCompleted onWorkCompleted, OnError onError)
		{
			this.status = Status.Idle;
			this.doInBackground = doInBackground;
			this.onWorkCompleted = onWorkCompleted;
			this.onError = onError;
			this.retryCountRemaining = retryCount;
			this.retryCount = retryCount;
			this.retryDelay = retryDelay;
		}

		public void Run(params object[] input)
		{
			if (status == Status.Running) {
				return;
			}

			coroutine = CreateCoroutine (input);
			EditorApplication.update += Update;
		}

		// Returns whether or not the thread was stopped
		public bool ForceStop()
		{
#pragma warning disable 420
			// Interlocked.CompareExchange (address, newValue, expectedValue) == originalValue
			if (Interlocked.CompareExchange (ref _status, (int)Status.Stopped, (int)Status.Running) == (int)Status.Running) {
				thread.Abort ();
				return true;
			}
#pragma warning restore 420

			return false;
		}

		private IEnumerator CreateCoroutine(params object[] input)
		{
			StartBackgroundTask (input);

			while (status == Status.Running) {
				yield return null;
			}

			if (status == Status.Error) {
				if (RetriesExhausted ()) {
					// Give up.
					onError (new RetryFailureException (exception.Message));
				} else if (onError (exception) == ErrorRecovery.Retry) {
					retryCountRemaining -= 1;
					Run (input);
				}
			} else {
				onWorkCompleted (result);
			}
		}

		private void StartBackgroundTask(params object[] input)
		{
			status = Status.Running;
			thread = new Thread (unused => {
				Thread.Sleep (retryDelay);
				RunBackgroundTask (input);
			});
			thread.Start ();
		}

		private void RunBackgroundTask(params object[] input)
		{
			try {
				result = doInBackground (input);
				status = Status.Done;
			} catch (System.Exception e) {
				status = Status.Error;
				exception = e;
			}
		}
		
		private void Stop()
		{
			EditorApplication.update -= Update;
		}
		
		private void Update()
		{
			if (!coroutine.MoveNext ()) {
				Stop ();
			}
		}

		private bool RetriesExhausted()
		{
			return retryCount != 0 && retryCountRemaining == 0;
		}
	}
}
