using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AssetStudio
{
    public sealed class TextAsset : NamedObject
    {
        public byte[] m_Script;

        public TextAsset(ObjectReader reader) : base(reader)
        {
            m_Script = reader.ReadUInt8Array();
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
