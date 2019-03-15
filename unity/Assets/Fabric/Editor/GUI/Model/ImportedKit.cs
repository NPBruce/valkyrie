namespace Fabric.Internal.Editor.Model
{
	using Fabric.Internal.Editor.Controller;

	/**
	 * This class is a representation of a kit that has been downloaded and
	 * included in a project, but has not necessarily been fully activated.
	 * Instances of this class are instantiated when a Controller for the
	 * given kit is present and visible via reflection.
	 */
	internal class ImportedKit
	{
		public readonly string Name;
		public readonly KitController Instance;

		public ImportedKit(string name, object instance)
		{
			Name = name;
			Instance = (KitController)instance;
		}
	}
}
