using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFGAppImport
{
    public class FFGImport
    {
        public static int version = 2;
        public GameType type;
        public Platform platform;
        public string path;
        public string apkPath = "";
        public string packageVersion = "";
        public bool editor;
        FetchContent fc;

        public FFGImport(GameType t, Platform p, string contentPath, bool e = false)
        {
            type = t;
            platform = p;
            path = contentPath + t.ToString() + "/import";
            editor = e;
        }

        public bool Inspect()
        {
            fc = new FetchContent(this);
            return true;
        }

        public bool NeedImport()
        {
            return fc.NeedImport();
        }

        public bool ImportAvailable()
        {
            return fc.importAvailable;
        }

        public bool Import(string import_path)
        { 
            fc.Import(import_path);
            return true;
        }

    }

    public enum GameType
    {
        D2E,
        MoM,
        IA
    }

    public enum Platform
    {
        Windows,
        MacOS,
        Linux,
        Android
    }
}
