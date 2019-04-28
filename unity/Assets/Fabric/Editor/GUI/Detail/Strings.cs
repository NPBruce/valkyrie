namespace Fabric.Internal.Editor.Detail
{
	using UnityEngine;
	using System.Collections;
	
	public class Strings
	{
		public static string Unwrap(string text, char starting, char ending)
		{
			int s = text.IndexOf (starting);
			int e = text.LastIndexOf (ending);

			if (s < 0 || e < 0 || s > e) {
				return text;
			}

			return text.Remove (e).Substring (s + 1);
		}
	}
}
