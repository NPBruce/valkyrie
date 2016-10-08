// This code is originally from UnityStudio, adapted here to suit Valkyrie

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class TextAsset
    {
        public string m_Name;
        public byte[] m_Script;
        public string m_PathName;

        public TextAsset(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;
            preloadData.extension = ".txt";

            if (sourceFile.platform == -2)
            {
                a_Stream.ReadUInt32();
                sourceFile.ReadPPtr();
                sourceFile.ReadPPtr();
            }

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());

            int m_Script_size = a_Stream.ReadInt32();

            if (readSwitch) //asset is read for preview or export
            {
                m_Script = new byte[m_Script_size];
                a_Stream.Read(m_Script, 0, m_Script_size);
                if (m_Script[0] == 60 || (m_Script[0] == 239 && m_Script[1] == 187 && m_Script[2] == 191 && m_Script[3] == 60)) { preloadData.extension = ".xml"; }
            }
            else
            {
                byte lzmaTest = a_Stream.ReadByte();
                if (lzmaTest == 93)
                {
                    a_Stream.Position += 4;
                    preloadData.exportSize = a_Stream.ReadInt32(); //actualy int64
                    a_Stream.Position -= 8;
                }
                else { preloadData.exportSize = m_Script_size; }

                a_Stream.Position += m_Script_size - 1;

                if (m_Name != "") { preloadData.Text = m_Name; }
                else { preloadData.Text = preloadData.TypeString + " #" + preloadData.uniqueID; }
                preloadData.subItems.AddRange(new string[] { preloadData.TypeString, preloadData.exportSize.ToString() });
            }
            a_Stream.AlignStream(4);

            m_PathName = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
        }

        public byte[] Deobfuscate(int key)
        {
            if (key == 0) return m_Script;

            byte[] retData = new byte[m_Script.Length];

            int byteOffset = 0;
            byte[] intData = new byte[4];
            for (int i = 0; i < m_Script.Length; i++)
            {
                intData[byteOffset++] = m_Script[i];

                if (byteOffset >= 4)
                {
                    int num = (int)intData[0] << 24 | (int)intData[1] << 16 | (int)intData[2] << 8 | (int)intData[3];
                    num ^= key;
                    byteOffset = 0;

                    retData[i - 3] = (byte)(num >> 24);
                    retData[i - 2] = (byte)(num >> 16 & 255);
                    retData[i - 1] = (byte)(num >> 8 & 255);
                    retData[i] = (byte)(num & 255);
                    byteOffset = 0;
                }
            }
            return retData;
        }
    }
}
