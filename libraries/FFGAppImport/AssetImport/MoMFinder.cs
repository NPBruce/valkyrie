using System.Collections;

namespace FFGAppImport
{
    // Details for FFG MoM app
    public class MoMFinder : AppFinder
    {
        public MoMFinder(Platform p) : base(p)
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
            return "478980";
        }
        // Where to store imported data
        override public string Destination()
        {
            return "MoM";
        }
        override public string DataDirectory()
        {
            if (platform == Platform.MacOS)
            {
                return "/Contents/Resources/Data";
            }
            return "/Mansions of Madness_Data";
        }
        override public string Executable()
        {
            if (platform == Platform.MacOS)
            {
                return "Mansions of Madness.app";
            }
            return "Mansions of Madness.exe";
        }

        // MoM uses this key to obfuscate text
        override public int ObfuscateKey()
        {
            return 68264378;
        }
    }
}