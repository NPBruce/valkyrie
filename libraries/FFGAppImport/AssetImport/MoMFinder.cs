using System.Collections.Generic;
using System.IO;
using ValkyrieTools;

namespace FFGAppImport
{
    // Details for FFG MoM app
    public class MoMFinder : AppFinder
    {
        protected string obbPath;

        public MoMFinder(Platform p) : base(p)
        {
        }

        // If the installed app isn't this or higher don't import
        override public string RequiredFFGVersion()
        {
            return "1.4.1";
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
            else if (platform == Platform.Android)
            {
                return "";
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

        public override string ObbPath()
        {
            if (obbPath == null) // try this only once
            {
                string location = Android.GetStorage() + "/Android/obb/com.fantasyflightgames.mom";
                if (!Directory.Exists(location))
                {
                    return "";
                }
                var files = new List<string>(Directory.GetFiles(location));
                string file = files.Find(x => x.EndsWith(".com.fantasyflightgames.mom.obb"));
                obbPath = file ?? "";
            }
            return obbPath;
        }
    }
}