using System.Collections.Generic;
using System.IO;
using ValkyrieTools;

namespace FFGAppImport
{
    public class RtLFinder : AppFinder
    {
        protected string obbPath;

        public RtLFinder(Platform p) : base(p)
        {
        }

        // If the installed app isn't this or higher don't import
        override public string RequiredFFGVersion()
        {
            return "1.4.0";
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
            else if (platform == Platform.Android)
            {
                return "";
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

        public override string ObbPath()
        {
            if (obbPath != null) // try this only once
                return obbPath;
            obbPath = GetObbPath("Android/obb/com.fantasyflightgames.rtl", ".com.fantasyflightgames.rtl.obb");
            return obbPath;
        }
    }
}