namespace Fabric.Internal.Editor.Update.Dependency
{
	using UnityEngine;
	using System.Collections.Generic;

	internal static class DependencyGraphLegacy
	{
		private static readonly Dictionary<string, string> initialDependencyMetadata = new Dictionary<string, string> () {
			{ "Crashlytics", "[{\"name\":\"Fabric-Android\",\"version\":\"1.3.10\"},{\"name\":\"Fabric-iOS\",\"version\":\"1.6.6\"},{\"name\":\"Answers-Android\",\"version\":\"1.3.6\"},{\"name\":\"Beta-Android\",\"version\":\"1.1.4\"}]" },
			{ "Twitter", "[{\"name\":\"Tweet-Composer-Android\",\"version\":\"1.0.3\"},{\"name\":\"Twitter-Core-Android\",\"version\":\"1.6.4\"},{\"name\":\"Fabric-Android\",\"version\":\"1.3.10\"},{\"name\":\"Twitter-Core-iOS\",\"version\":\"1.15.1\"},{\"name\":\"Fabric-iOS\",\"version\":\"1.6.6\"},{\"name\":\"TwitterKit-Wrapper-Android\",\"version\":\"0.1.0\"},{\"name\":\"TwitterKit-Wrapper-iOS\",\"version\":\"0.0.0\"}]" }
		};

		// Get the initial kit dependency metadata for when dependency management was first introduced.
		public static string InitialDependencyFor(string kitName)
		{
			if (!initialDependencyMetadata.ContainsKey (kitName)) {
				return "";
			}

			return initialDependencyMetadata [kitName];
		}
		
	}
}
