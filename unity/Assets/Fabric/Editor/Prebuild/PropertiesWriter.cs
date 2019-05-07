namespace Fabric.Internal.Editor.Prebuild
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	/**
	 * Writes a Dictionary to a Java-parsable Properties format
	 */
	public static class PropertiesWriter
	{

		public static void Write(Dictionary<string, string> properties, StreamWriter writer, string comment)
		{
			if (comment != null) {
				writer.WriteLine(comment);
			}

			writer.WriteLine("# " + DateTime.Now.ToString("R"));

			foreach (KeyValuePair<string, string> entry in properties) {
				writer.WriteLine (escapeString (entry.Key, true) + "=" + escapeString (entry.Value, false));
			}

			writer.Flush();
		}

		private static string escapeString(string s, bool isKey)
		{
			StringBuilder sb = new StringBuilder ();
			int i = 0;
			if (!isKey && i < s.Length && s [i] == ' ') {
				sb.Append("\\ ");
				i++;
			}

			for (; i < s.Length; ++i) {
				char c = s [i];
				switch (c) {
				case '\t':
					sb.Append ("\\t");
					break;
				case '\n':
					sb.Append ("\\n");
					break;
				case '\f':
					sb.Append ("\\f");
					break;
				case '\r':
					sb.Append ("\\r");
					break;
				default:
					if ("\\#!=:".IndexOf (c) >= 0 || (isKey && c == ' ')) {
						sb.Append ('\\');
					}
					if (c >= ' ' && c <= '~') {
						sb.Append (c);
					} else {
						string hex = Convert.ToInt32(c).ToString ("x4");
						sb.Append ("\\u").Append (hex);
					}
					break;
				}
			}

			return sb.ToString ();
		}
	}
}
