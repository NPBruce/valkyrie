// This code is originally from UnityStudio, adapted here to suit Valkyrie

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class unityFont
    {
        public string m_Name;
        public byte[] m_FontData;

        public unityFont(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            if (sourceFile.platform == -2)
            {
                a_Stream.ReadUInt32();
                sourceFile.ReadPPtr();
                sourceFile.ReadPPtr();
            }

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());

            if (readSwitch)
            {
                a_Stream.ReadInt32();

                if (sourceFile.version[0] <= 3)
                {
                    a_Stream.ReadInt32();
                    a_Stream.ReadInt32();
                }

                a_Stream.ReadSingle();
                a_Stream.ReadSingle();

                if (sourceFile.version[0] <= 3)
                {
                    int m_PerCharacterKerning_size = a_Stream.ReadInt32();
                    for (int i = 0; i < m_PerCharacterKerning_size; i++)
                    {
                        a_Stream.ReadInt32();
                        a_Stream.ReadSingle();
                    }
                }
                else
                {
                    a_Stream.ReadInt32();
                    a_Stream.ReadInt32();
                }

                a_Stream.ReadInt32();
                sourceFile.ReadPPtr();

                int m_CharacterRects_size = a_Stream.ReadInt32();
                for (int i = 0; i < m_CharacterRects_size; i++)
                {
                    a_Stream.ReadInt32();
                    //Rectf uv
                    a_Stream.ReadSingle();
                    a_Stream.ReadSingle();
                    a_Stream.ReadSingle();
                    a_Stream.ReadSingle();
                    //Rectf vert
                    a_Stream.ReadSingle();
                    a_Stream.ReadSingle();
                    a_Stream.ReadSingle();
                    a_Stream.ReadSingle();
                    a_Stream.ReadSingle();

                    if (sourceFile.version[0] >= 4)
                    {
                        a_Stream.ReadBoolean();
                        a_Stream.Position += 3;
                    }
                }

                sourceFile.ReadPPtr();

                int m_KerningValues_size = a_Stream.ReadInt32();
                for (int i = 0; i < m_KerningValues_size; i++)
                {
                    a_Stream.ReadInt16();
                    a_Stream.ReadInt16();
                    a_Stream.ReadSingle();
                }

                if (sourceFile.version[0] <= 3)
                {
                    a_Stream.ReadBoolean();
                    a_Stream.Position += 3; //4 byte alignment
                }
                else { a_Stream.ReadSingle(); }

                int m_FontData_size = a_Stream.ReadInt32();
                if (m_FontData_size > 0)
                {
                    m_FontData = new byte[m_FontData_size];
                    a_Stream.Read(m_FontData, 0, m_FontData_size);

                    if (m_FontData[0] == 79 && m_FontData[1] == 84 && m_FontData[2] == 84 && m_FontData[3] == 79)
                    { preloadData.extension = ".otf"; }
                    else { preloadData.extension = ".ttf"; }

                }

                a_Stream.ReadSingle();//problem here in minifootball
                a_Stream.ReadSingle();
                a_Stream.ReadUInt32();

                int m_FontNames = a_Stream.ReadInt32();
                for (int i = 0; i < m_FontNames; i++)
                {
                    a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                }

                if (sourceFile.version[0] >= 4)
                {
                    int m_FallbackFonts = a_Stream.ReadInt32();
                    for (int i = 0; i < m_FallbackFonts; i++)
                    {
                        sourceFile.ReadPPtr();
                    }

                    a_Stream.ReadInt32();
                }
            }
            else
            {
                if (m_Name != "") { preloadData.Text = m_Name; }
                else { preloadData.Text = preloadData.TypeString + " #" + preloadData.uniqueID; }
                preloadData.subItems.AddRange(new string[] { preloadData.TypeString, preloadData.exportSize.ToString() });
            }
        }
    }
}
