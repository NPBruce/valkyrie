using UnityEngine;

public enum Platform
{
	OSX,
	WIN
}

public class SystemHelper
{
	private Platform _platform;

	public SystemHelper()
	{
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.OSXPlayer || platform == RuntimePlatform.OSXEditor)
		{
			_platform = Platform.OSX;
		}
		else {
			_platform = Platform.WIN;
		}
	}

	public Platform getPlatform()
	{
		return _platform;
	}
}
