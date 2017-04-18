﻿// This code is originally from UnityStudio, adapted here to suit Valkyrie

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class Texture2D
    {
        public string m_Name;
        public int m_Width;
        public int m_Height;
        public int m_CompleteImageSize;
        public int m_TextureFormat;
        public bool m_MipMap = false;
        public bool m_IsReadable;
        public bool m_ReadAllowed;
        public int m_ImageCount;
        public int m_TextureDimension;
        //m_TextureSettings
        public int m_FilterMode;
        public int m_Aniso;
        public float m_MipBias;
        public int m_WrapMode;
        public int m_LightmapFormat;
        public int m_ColorSpace;
        public byte[] image_data;


        public int dwFlags = 0x1 + 0x2 + 0x4 + 0x1000;
        //public int dwHeight;
        //public int dwWidth;
        public int dwPitchOrLinearSize = 0x0;
        public int dwMipMapCount = 0x1;
        public int dwSize = 0x20;
        public int dwFlags2;
        public int dwFourCC = 0x0;
        public int dwRGBBitCount;
        public int dwRBitMask;
        public int dwGBitMask;
        public int dwBBitMask;
        public int dwABitMask;
        public int dwCaps = 0x1000;
        public int dwCaps2 = 0x0;
        public int m_offset;
        public string m_source;

        public int pvrVersion = 0x03525650;
        public int pvrFlags = 0x0;
        public long pvrPixelFormat;
        public int pvrColourSpace = 0x0;
        public int pvrChannelType = 0x0;
        //public int pvrHeight;
        //public int pvrWidth;
        public int pvrDepth = 0x1;
        public int pvrNumSurfaces = 0x1; //For texture arrays
        public int pvrNumFaces = 0x1; //For cube maps
        //public int pvrMIPMapCount;
        public int pvrMetaDataSize = 0x0;

        public int image_data_size;

        public Texture2D(AssetPreloadData preloadData, bool readSwitch)
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
            m_Width = a_Stream.ReadInt32();
            m_Height = a_Stream.ReadInt32();
            m_CompleteImageSize = a_Stream.ReadInt32();
            m_TextureFormat = a_Stream.ReadInt32();

            if (sourceFile.version[0] < 5 || (sourceFile.version[0] == 5 && sourceFile.version[1] < 2))
            { m_MipMap = a_Stream.ReadBoolean(); }
            else
            {
                dwFlags += 0x20000;
                dwMipMapCount = a_Stream.ReadInt32();//is this with or without main image?
                dwCaps += 0x400008;
            }

            m_IsReadable = a_Stream.ReadBoolean(); //2.6.0 and up
            m_ReadAllowed = a_Stream.ReadBoolean(); //3.0.0 and up
            a_Stream.AlignStream(4);

            m_ImageCount = a_Stream.ReadInt32();
            m_TextureDimension = a_Stream.ReadInt32();
            //m_TextureSettings
            m_FilterMode = a_Stream.ReadInt32();
            m_Aniso = a_Stream.ReadInt32();
            m_MipBias = a_Stream.ReadSingle();
            m_WrapMode = a_Stream.ReadInt32();

            if (sourceFile.version[0] >= 3)
            {
                m_LightmapFormat = a_Stream.ReadInt32();
                if (sourceFile.version[0] >= 4 || sourceFile.version[1] >= 5) { m_ColorSpace = a_Stream.ReadInt32(); } //3.5.0 and up
            }

            image_data_size = a_Stream.ReadInt32();

            if (image_data_size == 0 && sourceFile.version[0] >= 5 && sourceFile.version[1] >= 3)
            {
                m_offset = a_Stream.ReadInt32();
                image_data_size = a_Stream.ReadInt32();
                int resourceFileNameSize = a_Stream.ReadInt32();
                m_source = a_Stream.ReadAlignedString(resourceFileNameSize);
                m_source = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(sourceFile.filePath), m_source);
            }

            if (m_MipMap)
            {
                dwFlags += 0x20000;
                dwMipMapCount = Convert.ToInt32(Math.Log(Math.Max(m_Width, m_Height)) / Math.Log(2));
                dwCaps += 0x400008;
            }

            if (readSwitch)
            {

                image_data = new byte[image_data_size];

                if (m_source == null)
                {
                    a_Stream.Read(image_data, 0, image_data_size);
                }
                else if (System.IO.File.Exists(m_source))
                {
                    image_data = new byte[image_data_size];
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(m_source)))
                    {
                        reader.BaseStream.Position = m_offset;
                        reader.Read(image_data, 0, image_data_size);
                        reader.Close();
                    }
                }

                switch (m_TextureFormat)
                {
                    case 1: //Alpha8
                        {
                            dwFlags2 = 0x2;
                            dwRGBBitCount = 0x8;
                            dwRBitMask = 0x0;
                            dwGBitMask = 0x0;
                            dwBBitMask = 0x0;
                            dwABitMask = 0xFF;
                            break;
                        }
                    case 2: //A4R4G4B4
                        {
                            if (sourceFile.platform == 11) //swap bytes for Xbox confirmed, PS3 not encountered
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte b0 = image_data[i * 2];
                                    image_data[i * 2] = image_data[i * 2 + 1];
                                    image_data[i * 2 + 1] = b0;
                                }
                            }
                            else if (sourceFile.platform == 13) //swap bits for android
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte[] argb = BitConverter.GetBytes((BitConverter.ToInt32((new byte[4] { image_data[i * 2], image_data[i * 2 + 1], image_data[i * 2], image_data[i * 2 + 1] }), 0)) >> 4);
                                    image_data[i * 2] = argb[0];
                                    image_data[i * 2 + 1] = argb[1];
                                }
                            }

                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x10;
                            dwRBitMask = 0xF00;
                            dwGBitMask = 0xF0;
                            dwBBitMask = 0xF;
                            dwABitMask = 0xF000;
                            break;
                        }
                    case 3: //B8G8R8 //confirmed on X360, iOS //PS3 unsure
                        {
                            for (int i = 0; i < (image_data_size / 3); i++)
                            {
                                byte b0 = image_data[i * 3];
                                image_data[i * 3] = image_data[i * 3 + 2];
                                //image_data[i * 3 + 1] stays the same
                                image_data[i * 3 + 2] = b0;

                            }

                            dwFlags2 = 0x40;
                            dwRGBBitCount = 0x18;
                            dwRBitMask = 0xFF0000;
                            dwGBitMask = 0xFF00;
                            dwBBitMask = 0xFF;
                            dwABitMask = 0x0;
                            break;
                        }
                    case 4: //G8R8A8B8 //confirmed on X360, iOS
                        {
                            for (int i = 0; i < (image_data_size / 4); i++)
                            {
                                byte b0 = image_data[i * 4];
                                image_data[i * 4] = image_data[i * 4 + 2];
                                //image_data[i * 4 + 1] stays the same
                                image_data[i * 4 + 2] = b0;
                                //image_data[i * 4 + 3] stays the same

                            }

                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x20;
                            dwRBitMask = 0xFF0000;
                            dwGBitMask = 0xFF00;
                            dwBBitMask = 0xFF;
                            dwABitMask = -16777216;
                            break;
                        }
                    case 5: //B8G8R8A8 //confirmed on X360, PS3, Web, iOS
                        {
                            for (int i = 0; i < (image_data_size / 4); i++)
                            {
                                byte b0 = image_data[i * 4];
                                byte b1 = image_data[i * 4 + 1];
                                image_data[i * 4] = image_data[i * 4 + 3];
                                image_data[i * 4 + 1] = image_data[i * 4 + 2];
                                image_data[i * 4 + 2] = b1;
                                image_data[i * 4 + 3] = b0;

                            }

                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x20;
                            dwRBitMask = 0xFF0000;
                            dwGBitMask = 0xFF00;
                            dwBBitMask = 0xFF;
                            dwABitMask = -16777216;
                            break;
                        }
                    case 7: //R5G6B5 //confirmed switched on X360; confirmed on iOS
                        {
                            if (sourceFile.platform == 11)
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte b0 = image_data[i * 2];
                                    image_data[i * 2] = image_data[i * 2 + 1];
                                    image_data[i * 2 + 1] = b0;
                                }
                            }

                            dwFlags2 = 0x40;
                            dwRGBBitCount = 0x10;
                            dwRBitMask = 0xF800;
                            dwGBitMask = 0x7E0;
                            dwBBitMask = 0x1F;
                            dwABitMask = 0x0;
                            break;
                        }
                    case 10: //DXT1
                        {
                            if (sourceFile.platform == 11) //X360 only, PS3 not
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte b0 = image_data[i * 2];
                                    image_data[i * 2] = image_data[i * 2 + 1];
                                    image_data[i * 2 + 1] = b0;
                                }
                            }

                            if (m_MipMap) { dwPitchOrLinearSize = m_Height * m_Width / 2; }
                            dwFlags2 = 0x4;
                            dwFourCC = 0x31545844;
                            dwRGBBitCount = 0x0;
                            dwRBitMask = 0x0;
                            dwGBitMask = 0x0;
                            dwBBitMask = 0x0;
                            dwABitMask = 0x0;
                            break;
                        }
                    case 12: //DXT5
                        {
                            if (sourceFile.platform == 11) //X360, PS3 not
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte b0 = image_data[i * 2];
                                    image_data[i * 2] = image_data[i * 2 + 1];
                                    image_data[i * 2 + 1] = b0;
                                }
                            }

                            if (m_MipMap) { dwPitchOrLinearSize = m_Height * m_Width / 2; }
                            dwFlags2 = 0x4;
                            dwFourCC = 0x35545844;
                            dwRGBBitCount = 0x0;
                            dwRBitMask = 0x0;
                            dwGBitMask = 0x0;
                            dwBBitMask = 0x0;
                            dwABitMask = 0x0;
                            break;
                        }
                    case 13: //R4G4B4A4, iOS (only?)
                        {
                            for (int i = 0; i < (image_data_size / 2); i++)
                            {
                                byte[] argb = BitConverter.GetBytes((BitConverter.ToInt32((new byte[4] { image_data[i * 2], image_data[i * 2 + 1], image_data[i * 2], image_data[i * 2 + 1] }), 0)) >> 4);
                                image_data[i * 2] = argb[0];
                                image_data[i * 2 + 1] = argb[1];
                            }

                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x10;
                            dwRBitMask = 0xF00;
                            dwGBitMask = 0xF0;
                            dwBBitMask = 0xF;
                            dwABitMask = 0xF000;
                            break;
                        }
                    case 28: //DXT1 Crunched
                    case 29: //DXT1 Crunched
                        break;
                    case 30: //PVRTC_RGB2
                        {
                            pvrPixelFormat = 0x0;
                            break;
                        }
                    case 31: //PVRTC_RGBA2
                        {
                            pvrPixelFormat = 0x1;
                            break;
                        }
                    case 32: //PVRTC_RGB4
                        {
                            pvrPixelFormat = 0x2;
                            break;
                        }
                    case 33: //PVRTC_RGBA4
                        {
                            pvrPixelFormat = 0x3;
                            break;
                        }
                    case 34: //ETC_RGB4
                        {
                            pvrPixelFormat = 0x16;
                            break;
                        }
                }
            }
            else
            {
                preloadData.InfoText = "Width: " + m_Width.ToString() + "\nHeight: " + m_Height.ToString() + "\nFormat: ";
                preloadData.exportSize = image_data_size;

                switch (m_TextureFormat)
                {
                    case 1: preloadData.InfoText += "Alpha8"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 2: preloadData.InfoText += "ARGB 4.4.4.4"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 3: preloadData.InfoText += "BGR 8.8.8"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 4: preloadData.InfoText += "GRAB 8.8.8.8"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 5: preloadData.InfoText += "BGRA 8.8.8.8"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 7: preloadData.InfoText += "RGB 5.6.5"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 10: preloadData.InfoText += "DXT1"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 12: preloadData.InfoText += "DXT5"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 13: preloadData.InfoText += "RGBA 4.4.4.4"; preloadData.extension = ".dds"; preloadData.exportSize += 128; break;
                    case 28: preloadData.InfoText += "DXT1 Crunched"; preloadData.extension = ".crn"; break;
                    case 29: preloadData.InfoText += "DXT5 Crunched"; preloadData.extension = ".crn"; break;
                    case 30: preloadData.InfoText += "PVRTC_RGB2"; preloadData.extension = ".pvr"; preloadData.exportSize += 52; break;
                    case 31: preloadData.InfoText += "PVRTC_RGBA2"; preloadData.extension = ".pvr"; preloadData.exportSize += 52; break;
                    case 32: preloadData.InfoText += "PVRTC_RGB4"; preloadData.extension = ".pvr"; preloadData.exportSize += 52; break;
                    case 33: preloadData.InfoText += "PVRTC_RGBA4"; preloadData.extension = ".pvr"; preloadData.exportSize += 52; break;
                    case 34: preloadData.InfoText += "ETC_RGB4"; preloadData.extension = ".pvr"; preloadData.exportSize += 52; break;
                    default: preloadData.InfoText += "unknown"; preloadData.extension = ".tex"; break;
                }

                switch (m_FilterMode)
                {
                    case 0: preloadData.InfoText += "\nFilter Mode: Point "; break;
                    case 1: preloadData.InfoText += "\nFilter Mode: Bilinear "; break;
                    case 2: preloadData.InfoText += "\nFilter Mode: Trilinear "; break;

                }

                preloadData.InfoText += "\nAnisotropic level: " + m_Aniso.ToString() + "\nMip map bias: " + m_MipBias.ToString();

                switch (m_WrapMode)
                {
                    case 0: preloadData.InfoText += "\nWrap mode: Repeat"; break;
                    case 1: preloadData.InfoText += "\nWrap mode: Clamp"; break;
                }

                if (m_Name != "") { preloadData.Text = m_Name; }
                else { preloadData.Text = preloadData.TypeString + " #" + preloadData.uniqueID; }
                preloadData.subItems.AddRange(new string[] { preloadData.TypeString, preloadData.exportSize.ToString() });
            }
        }
    }
}
