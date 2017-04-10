
namespace FFGAppImport
{
    public class RtLFinder : AppFinder
    {
        public RtLFinder(Platform p) : base(p)
        {
        }

        // If the installed app isn't this or higher don't import
        override public string RequiredFFGVersion()
        {
            return "1.3.0";
        }
        // Steam app ID
        override public string AppId()
        {
            return "477200";
        }
        // Where to store imported data
        override public string Destination()
        {
            return "D2E";
        }

        override public string DataDirectory()
        {
            if (platform == Platform.MacOS)
            {
                return "/Contents/Resources/Data";
            }
            return "/Road to Legend_Data";
        }
        override public string Executable()
        {
            if (platform == Platform.MacOS)
            {
                return "Road to Legend.app";
            }
            return "Road to Legend.exe";
        }
        // RtL does not obfuscate text
        override public int ObfuscateKey()
        {
            return 0;
        }
    }
}