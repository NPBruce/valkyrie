namespace Fabric.Internal.Editor.Net
{
	using System.Net.NetworkInformation;
	using System.Net;

	internal class Utils
	{
		public static bool IsNetworkUnavailableFrom(System.Exception e)
		{
			return e is WebException ?
				IsNetworkUnavailableFrom (e as WebException) :
				IsNetworkApproximatelyUnavailable ();
		}

		public static bool IsNetworkUnavailableFrom(WebException e)
		{
			return IsNetworkApproximatelyUnavailable () ||
				e.Status == WebExceptionStatus.NameResolutionFailure ||
				e.Status == WebExceptionStatus.Timeout ||
				e.Status == WebExceptionStatus.ConnectFailure;
		}

		public static bool IsNetworkApproximatelyUnavailable()
		{
			return !NetworkInterface.GetIsNetworkAvailable ();
		}
	}
}
