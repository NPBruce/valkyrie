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

        // Fetch Deobfuscated text (XOR with key)
        // Used in MoM app
        public byte[] Deobfuscate(int key)
        {
            // No key, return data
            if (key == 0) return m_Script;

            // If it looks like text already, assume not obfuscated and return
            if (!isBinary()) return m_Script;

            // Create new data array
            byte[] retData = new byte[m_Script.Length];

            int byteOffset = 0;
            // XOR 4 bytes (32 bits) at a time
            byte[] intData = new byte[4];
            // Loop through text
            for (int i = 0; i < m_Script.Length; i++)
            {
                // Save 4 bytes one at a time
                intData[byteOffset++] = m_Script[i];

                // Once we have 4 bytes XOR them
                if (byteOffset >= 4)
                {
                    // Add bytes together
                    int num = (int)intData[0] << 24 | (int)intData[1] << 16 | (int)intData[2] << 8 | (int)intData[3];
                    // XOR
                    num ^= key;
                    byteOffset = 0;

                    // Shuffle byte order
                    retData[i - 3] = (byte)(num >> 24);
                    retData[i - 2] = (byte)(num >> 16 & 255);
                    retData[i - 1] = (byte)(num >> 8 & 255);
                    retData[i] = (byte)(num & 255);
                    byteOffset = 0;
                }
            }
            return retData;
        }

        // Guess if the text is obfuscated (doesn't look like text)
        // FIXME this is probably too slow? impact untested
        public bool isBinary()
        {
            if (m_Script.Length == 0) return false;

            // Look for control characters
            for (int i = 0; i < m_Script.Length; i++)
            {
                if (isControlChar(m_Script[i]))
                {
                    // Control characters shouldn't exist in text
                    return true;
                }
            }
            // Must be text
            return false;
        }

        // Check if a characted is a control character (not normal text)
        public bool isControlChar(int ch)
        {
            return (ch > Chars.NUL && ch < Chars.BS)
                || (ch > Chars.CR && ch < Chars.SUB);
        }

        // List of control characters
        public class Chars
        {
            public static char NUL = (char)0; // Null char
            public static char BS = (char)8; // Back Space
            public static char CR = (char)13; // Carriage Return
            public static char SUB = (char)26; // Substitute
        }
    }
}
