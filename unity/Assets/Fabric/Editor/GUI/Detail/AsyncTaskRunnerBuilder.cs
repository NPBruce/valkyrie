namespace Fabric.Internal.Editor.Detail
{
	internal class AsyncTaskRunnerBuilder<R>
	{
		private AsyncTaskRunner<R>.DoInBackground doInBackground = args => { return default (R); };
		private AsyncTaskRunner<R>.OnWorkCompleted onWorkCompleted = result => {};
		private AsyncTaskRunner<R>.OnError onError = err => {
			return Detail.AsyncTaskRunner<R>.ErrorRecovery.Nothing;
		};
		private AsyncTaskRunner<R> runner = null;

		private uint retryCount = 0;
		private System.TimeSpan retryDelay = new System.TimeSpan (0);

		public AsyncTaskRunnerBuilder()
		{
		}

		public AsyncTaskRunnerBuilder<R> Do(AsyncTaskRunner<R>.DoInBackground doInBackground)
		{
			if (runner == null) {
				this.doInBackground = doInBackground;
			}
			return this;
		}
		
		public AsyncTaskRunnerBuilder<R> OnError(AsyncTaskRunner<R>.OnError onError)
		{
			if (runner == null) {
				this.onError = onError;
			}
			return this;
		}
		
		public AsyncTaskRunnerBuilder<R> OnCompletion(AsyncTaskRunner<R>.OnWorkCompleted onWorkCompleted)
		{
			if (runner == null) {
				this.onWorkCompleted = onWorkCompleted;
			}
			return this;
		}

		public AsyncTaskRunnerBuilder<R> Retry(uint retryCount, System.TimeSpan retryDelay)
		{
			this.retryCount = retryCount;
			this.retryDelay = retryDelay;

			return this;
		}

		public void Run(params object[] args)
		{
			if (runner == null) {
				runner = new AsyncTaskRunner<R> (retryCount, retryDelay, doInBackground, onWorkCompleted, onError);
			}

			runner.Run (args);
		}

		public bool Stop()
		{
			return runner != null && runner.ForceStop ();
		}
	}
}
