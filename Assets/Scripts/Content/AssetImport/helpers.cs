// This code is originally from UnityStudio, adapted here to suit Valkyrie

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Unity_Studio
{
    public class PPtr
    {
        //m_FileID 0 means current file
        public int m_FileID = -1;
        //m_PathID acts more like a hash in some games
        public long m_PathID = 0;
    }

    public static class PPtrHelpers
    {
        public static PPtr ReadPPtr(this AssetsFile sourceFile)
        {
            PPtr result = new PPtr();
            var a_Stream = sourceFile.a_Stream;

            int FileID = a_Stream.ReadInt32();
            if (FileID >= 0 && FileID < sourceFile.sharedAssetsList.Count)
            { result.m_FileID = sourceFile.sharedAssetsList[FileID].Index; }
            
            if (sourceFile.fileGen < 14) { result.m_PathID = a_Stream.ReadInt32(); }
            else { result.m_PathID = a_Stream.ReadInt64(); }

            return result;
        }
           
        public static bool TryGetPD(this List<AssetsFile> assetsfileList, PPtr m_elm, out AssetPreloadData result)
        {
            result = null;

            if (m_elm != null && m_elm.m_FileID >= 0 && m_elm.m_FileID < assetsfileList.Count)
            {
                AssetsFile sourceFile = assetsfileList[m_elm.m_FileID];

                //TryGetValue should be safe because m_PathID is 0 when initialized and PathID values range from 1
                if (sourceFile.preloadTable.TryGetValue(m_elm.m_PathID, out result)) { return true; }
            }

            return false;
        }

        public static bool TryGetTransform(this List<AssetsFile> assetsfileList, PPtr m_elm, out Transform m_Transform)
        {
            m_Transform = null;

            if (m_elm != null && m_elm.m_FileID >= 0 && m_elm.m_FileID < assetsfileList.Count)
            {
                AssetsFile sourceFile = assetsfileList[m_elm.m_FileID];

                if (sourceFile.TransformList.TryGetValue(m_elm.m_PathID, out m_Transform)) { return true; }
            }

            return false;
        }

        public static bool TryGetGameObject(this List<AssetsFile> assetsfileList, PPtr m_elm, out GameObject m_GameObject)
        {
            m_GameObject = null;

            if (m_elm != null && m_elm.m_FileID >= 0 && m_elm.m_FileID < assetsfileList.Count)
            {
                AssetsFile sourceFile = assetsfileList[m_elm.m_FileID];

                if (sourceFile.GameObjectList.TryGetValue(m_elm.m_PathID, out m_GameObject)) { return true; }
            }

            return false;
        }
    }
}
