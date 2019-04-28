namespace Fabric.Internal.Editor.Update
{
	using System.IO;

	public class FileUtils
	{
		public static readonly string Root = NormalizePathForPlatform (UnityEngine.Application.dataPath);

		public static string NormalizePathForPlatform(string path)
		{
			return path.Replace ('/', Path.DirectorySeparatorChar);
		}
		
		public static string PathRelativeToRoot(string root, string path)
		{
			return NormalizePathForPlatform (root + path);
		}
	}
}
