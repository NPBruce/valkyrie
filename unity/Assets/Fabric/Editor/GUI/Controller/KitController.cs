namespace Fabric.Internal.Editor.Controller
{
	using System;
	using Fabric.Internal.Editor.View;

	public enum KitControllerStatus { NextPage, LastPage, CurrentPage };
	internal interface KitController
	{
		KitControllerStatus PageFromState(out Page page);
		string DisplayName();
		Version Version();
	}
}