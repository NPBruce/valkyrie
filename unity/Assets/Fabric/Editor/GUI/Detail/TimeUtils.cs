namespace Fabric.Internal.Editor.Detail
{
	using System;

	internal static class TimeUtils
	{
		private static readonly DateTime Epoch = new DateTime (1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static double SecondsSinceEpoch
		{
			get {
				return ToEpochSeconds (DateTime.UtcNow);
			}
		}

		public static DateTime FromEpochSeconds(double totalSeconds)
		{
			return Epoch.AddSeconds (totalSeconds);
		}

		public static double ToEpochSeconds(DateTime timestamp)
		{
			return Math.Floor ((timestamp - Epoch).TotalSeconds);
		}
	}
}
