// This code is originally from UnityStudio, adapted here to suit Valkyrie

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    public class AssetPreloadData
    {
        public long m_PathID;
        public int Offset;
        public int Size;
        public int Type1;
        public ushort Type2;

        public string TypeString;
        public int exportSize;
        public string InfoText;
        public string extension;

        public AssetsFile sourceFile;
        public string uniqueID;

        public string Text;
        public List<string> subItems;

        public AssetPreloadData()
        {
            subItems = new List<string>();
        }
    }
}
