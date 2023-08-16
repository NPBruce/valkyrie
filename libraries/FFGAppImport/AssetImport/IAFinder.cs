using System.Collections.Generic;
using System.IO;
using ValkyrieTools;

namespace FFGAppImport
{
    public class IAFinder : AppFinder
    {
        protected string obbPath;

        public IAFinder(Platform p)
            : base(p)
        {
        }

        // If the installed app isn't this or higher don't import
        override public string RequiredFFGVersion()
        {
            return "1.0.0";
        }
        // Steam app ID
        override public string AppId()
        {
            return "703980";
        }
        // Where to store imported data
        override public string Destination()
        {
            return "IA";
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
            return "/Imperial Assault_Data";
        }
        override public string Executable()
        {
            if (platform == Platform.MacOS)
            {
                return "Imperial Assault.app";
            }
            return "Imperial Assault.exe";
        }
        // IA does not obfuscate text
        override public int ObfuscateKey()
        {
            return 0;
        }

        public override string DataPath()
        {
            return GetDataPath("com.fantasyflightgames.iaca");
        }

        public override string AuxDataPath()
        {
            return GetAuxDataPath("com.fantasyflightgames.iaca");
        }

        public override string ObbPath()
        {
            if (obbPath != null) // try this only once
                return obbPath;
            obbPath = GetObbPath("Android/obb/com.fantasyflightgames.iaca", ".com.fantasyflightgames.iaca.obb");
            return obbPath;
        }
    }
}