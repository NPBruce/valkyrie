namespace Fabric.Internal.Editor.Update
{
	using System.Net;
	using System;

	// WebClient for which we can set the request timeout
	internal sealed class TimeoutWebClient : WebClient
	{
		private readonly int timeout;
		
		public TimeoutWebClient(int timeout)
		{
			this.timeout = timeout;
		}
		
		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest w = base.GetWebRequest (uri);
			w.Timeout = timeout;
			return w;
		}
	}
}
