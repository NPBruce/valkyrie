namespace Fabric.Internal.Editor.Resources
{
	using UnityEngine;

	/**
	 * Deprecated. Use Fabric.Internal.Editor.Images.Loader
	 * This class is included for backward-compatibility with Crashlytics Kit v. 1.0.0
	 * If using Crashlytics Kit version > 1.0.0 this file can be removed.
	 */
	public class Manager
	{
		public static Texture2D Load(string resource)
		{
			return Images.Loader.Load (resource);
		}
	}
}
