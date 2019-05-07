using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Assembly-CSharp-Editor")]

namespace Fabric.Crashlytics.Internal
{
	using System;
	using Fabric.Internal.Runtime;
	using System.Diagnostics;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	internal class Impl
	{
		protected const string KitName = "Crashlytics";

		// Matches e.g. "Method(string arg1, int arg2)" or "Method (string arg1, int arg2)"
		private static readonly string FrameArgsRegex = @"\s?\(.*\)";

		// Matches e.g. "at Assets.Src.Gui.Menus.GameMenuController.ShowSkilltreeScreen (string arg1)"
		private static readonly string FrameRegexWithoutFileInfo = @"(?<class>[^\s]+)\.(?<method>[^\s\.]+)" + FrameArgsRegex;

		// Matches e.g.:
		//   "at Assets.Src.Gui.Menus.GameMenuController.ShowSkilltreeScreen () (at C:/Game_Project/Assets/Src/Gui/GameMenuController.cs:154)"
		//   "at SampleApp.Buttons.CrashlyticsButtons.CauseDivByZero () [0x00000] in /Some/Dir/Program.cs:567"
		private static readonly string FrameRegexWithFileInfo = FrameRegexWithoutFileInfo + @" .*[/|\\](?<file>.+):(?<line>\d+)";

		// Mono's hardcoded unknown file string (never localized)
		// See https://github.com/mono/mono/blob/b7a308f660de8174b64697a422abfc7315d07b8c/mcs/class/corlib/System.Diagnostics/StackFrame.cs#L146
		private static readonly string MonoFilenameUnknownString = "<filename unknown>";

		private static readonly string[] StringDelimiters = new string[] { Environment.NewLine };

		public static Impl Make()
		{
			#if UNITY_IOS && !UNITY_EDITOR
			return new IOSImpl ();
			#elif UNITY_ANDROID && !UNITY_EDITOR
			return new AndroidImpl ();
			#else
			return new Impl ();
			#endif
		}
		
		public virtual void SetDebugMode(bool mode)
		{
		}
		
		public virtual void Crash()
		{
			Utils.Log (KitName, "Method Crash () is unimplemented on this platform");
		}
		
		public virtual void ThrowNonFatal()
		{
			#if !UNITY_EDITOR
			string s = null;
			string l = s.ToLower ();
			Utils.Log (KitName, l);
			#else
			Utils.Log (KitName, "Method ThrowNonFatal () is not invokable from the context of the editor");
			#endif
		}
		
		public virtual void Log(string message)
		{
			Utils.Log (KitName, "Would log custom message if running on a physical device: " + message);
		}
		
		public virtual void SetKeyValue(string key, string value)
		{
			Utils.Log (KitName, "Would set key-value if running on a physical device: " + key + ":" + value);
		}
		
		public virtual void SetUserIdentifier(string identifier)
		{
			Utils.Log (KitName, "Would set user identifier if running on a physical device: " + identifier);
		}
		
		public virtual void SetUserEmail(string email)
		{
			Utils.Log (KitName, "Would set user email if running on a physical device: " + email);
		}
		
		public virtual void SetUserName(string name)
		{
			Utils.Log (KitName, "Would set user name if running on a physical device: " + name);
		}

		public virtual void RecordCustomException(string name, string reason, StackTrace stackTrace)
		{
			Utils.Log (KitName, "Would record custom exception if running on a physical device: " + name + ", " + reason);
		}

		public virtual void RecordCustomException(string name, string reason, string stackTraceString)
		{
			Utils.Log (KitName, "Would record custom exception if running on a physical device: " + name + ", " + reason);
		}

		private static Dictionary<string, string> ParseFrameString (string regex, string frameString)
		{
			var matches = Regex.Matches(frameString, regex);
			if (matches.Count < 1) {
				return null;
			}

			var match = matches[0];
			if (!match.Groups ["class"].Success || !match.Groups ["method"].Success) {
				return null;
			}

			string file = match.Groups ["file"].Success ? match.Groups ["file"].Value : match.Groups ["class"].Value;
			string line = match.Groups ["line"].Success ? match.Groups ["line"].Value : "0";

			if (file == MonoFilenameUnknownString) {
				file = match.Groups ["class"].Value;
				line = "0";
			}

			var dict = new Dictionary<string, string>();
			dict.Add("class", match.Groups["class"].Value);
			dict.Add("method", match.Groups["method"].Value);
			dict.Add("file", file);
			dict.Add("line", line);

			return dict;
		}

		private delegate Dictionary<string, string> FrameParser (string frameString);

		protected static Dictionary<string, string>[] ParseStackTraceString (string stackTraceString)
		{
			string regex = null;
			var result = new List< Dictionary<string, string> >();
			string[] splitStackTrace = stackTraceString.Split(StringDelimiters, StringSplitOptions.None);

			if (splitStackTrace.Length < 1) {
				return result.ToArray ();
			}

			if (Regex.Matches (splitStackTrace [0], FrameRegexWithFileInfo).Count == 1) {
				regex = FrameRegexWithFileInfo;
			} else if (Regex.Matches (splitStackTrace [0], FrameRegexWithoutFileInfo).Count == 1) {
				regex = FrameRegexWithoutFileInfo;
			} else {
				return result.ToArray();
			}

			foreach (var frameString in splitStackTrace) {
				var parsedDict = ParseFrameString (regex, frameString);
				if (parsedDict != null) {
					result.Add (parsedDict);
				}
			}

			return result.ToArray();
		}
	}
}
