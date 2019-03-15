namespace Fabric.Runtime
{
	using System.Collections.Generic;
	using System;
	using System.Reflection;

	public class Fabric
	{
		private static readonly Internal.Impl impl;

		static Fabric()
		{
			impl = Internal.Impl.Make ();
		}

		public static void Initialize()
		{
			string kitsToInitialize = impl.Initialize ();
			if (String.IsNullOrEmpty (kitsToInitialize)) {
				return;
			}

			string[] pairs = kitsToInitialize.Split (',');

			foreach (string kitMethod in pairs) {
				Initialize (kitMethod);
			}
		}

		internal static void Initialize(string kitMethod)
		{
			int separator = kitMethod.LastIndexOf ('.');

			string clazz = kitMethod.Substring (0, separator);
			string method = kitMethod.Substring (separator + 1);

			Type type = Type.GetType (clazz);
			if (type == null) {
				return;
			}

			// First, try to get the init method as public-static. If that doesn't work, try as private-static.
			MethodInfo initialize = type.GetMethod (method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (initialize == null) {
				return;
			}

			object instance = typeof(UnityEngine.ScriptableObject).IsAssignableFrom (type) ?
				UnityEngine.ScriptableObject.CreateInstance (type) :
				Activator.CreateInstance (type);

			if (instance == null) {
				return;
			}

			initialize.Invoke (instance, new object[] {});
		}
	}
}
