namespace Fabric.Internal.Editor.Model
{
	internal enum KitStatus
	{
		Available,     // Available for download.
		Imported,     // Downloaded, but not configured nor activated.
		Configured,   // Downloaded and configured, but not activated.
		Installed    // Completely installed and activated.
	}
}
