using System;
using ValkyrieTools;

namespace AssetStudio
{
    public class StreamingInfo
    {
        public long offset; //ulong
        public uint size;
        public string path;

        public StreamingInfo(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] >= 2020) //2020.1 and up
            {
                offset = reader.ReadInt64();
            }
            else
            {
                offset = reader.ReadUInt32();
            }
            size = reader.ReadUInt32();
            path = reader.ReadAlignedString();
        }
    }

    public class GLTextureSettings
    {
        public int m_FilterMode;
        public int m_Aniso;
        public float m_MipBias;
        public int m_WrapMode;

        public GLTextureSettings(ObjectReader reader)
        {
            var version = reader.version;

            m_FilterMode = reader.ReadInt32();
            m_Aniso = reader.ReadInt32();
            m_MipBias = reader.ReadSingle();
            if (version[0] >= 2017)//2017.x and up
            {
                m_WrapMode = reader.ReadInt32(); //m_WrapU
                int m_WrapV = reader.ReadInt32();
                int m_WrapW = reader.ReadInt32();
            }
            else
            {
                m_WrapMode = reader.ReadInt32();
            }
        }
    }

    public sealed class Texture2D : Texture
    {
        public int m_Width;
        public int m_Height;
        public TextureFormat m_TextureFormat;
        public bool m_MipMap;
        public int m_MipCount;
        public GLTextureSettings m_TextureSettings;
        public ResourceReader image_data;
        public StreamingInfo m_StreamData;

        public byte[] image_data_bytes;

        public int dwFlags = 0x1 + 0x2 + 0x4 + 0x1000;
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

        public int pvrVersion = 0x03525650;
        public int pvrFlags = 0x0;
        public long pvrPixelFormat;
        public int pvrColourSpace = 0x0;
        public int pvrChannelType = 0x0;
        public int pvrDepth = 0x1;
        public int pvrNumSurfaces = 0x1; //For texture arrays
        public int pvrNumFaces = 0x1; //For cube maps
        public int pvrMetaDataSize = 0x0;

        public Texture2D(ObjectReader reader) : base(reader)
        {
            m_Width = reader.ReadInt32();
            m_Height = reader.ReadInt32();
            var m_CompleteImageSize = reader.ReadInt32();
            if (version[0] >= 2020) //2020.1 and up
            {
                var m_MipsStripped = reader.ReadInt32();
            }
            m_TextureFormat = (TextureFormat)reader.ReadInt32();
            if (version[0] < 5 || (version[0] == 5 && version[1] < 2)) //5.2 down
            {
                m_MipMap = reader.ReadBoolean();
            }
            else
            {
                m_MipCount = reader.ReadInt32();
            }


            if (version[0] > 2 || (version[0] == 2 && version[1] >= 6)) //2.6.0 and up
            {
                var m_IsReadable = reader.ReadBoolean();
            }
            if (version[0] >= 2020) //2020.1 and up
            {
                var m_IsPreProcessed = reader.ReadBoolean();
            }
            if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) //2019.3 and up
            {
                var m_IgnoreMasterTextureLimit = reader.ReadBoolean();
            }
            if (version[0] >= 3) //3.0.0 - 5.4
            {
                if (version[0] < 5 || (version[0] == 5 && version[1] <= 4))
                {
                    var m_ReadAllowed = reader.ReadBoolean();
                }
            }
            if (version[0] > 2018 || (version[0] == 2018 && version[1] >= 2)) //2018.2 and up
            {
                var m_StreamingMipmaps = reader.ReadBoolean();
            }
            reader.AlignStream();
            if (version[0] > 2018 || (version[0] == 2018 && version[1] >= 2)) //2018.2 and up
            {
                var m_StreamingMipmapsPriority = reader.ReadInt32();
            }
            var m_ImageCount = reader.ReadInt32();
            var m_TextureDimension = reader.ReadInt32();
            m_TextureSettings = new GLTextureSettings(reader);
            if (version[0] >= 3) //3.0 and up
            {
                var m_LightmapFormat = reader.ReadInt32();
            }
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 5)) //3.5.0 and up
            {
                var m_ColorSpace = reader.ReadInt32();
            }
            if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) //2020.2 and up
            {
                var m_PlatformBlob = reader.ReadUInt8Array();
                reader.AlignStream();
            }
            var image_data_size = reader.ReadInt32();
            if (image_data_size == 0 && ((version[0] == 5 && version[1] >= 3) || version[0] > 5))//5.3.0 and up
            {
                m_StreamData = new StreamingInfo(reader);
                image_data_size = (int)m_StreamData.size;

            }

            ResourceReader resourceReader;
            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                resourceReader = new ResourceReader(m_StreamData.path, assetsFile, m_StreamData.offset, m_StreamData.size);
            }
            else
            {
                resourceReader = new ResourceReader(reader, reader.BaseStream.Position, image_data_size);
            }
            image_data = resourceReader;

            image_data_bytes = new byte[image_data_size];

            resourceReader.GetData(image_data_bytes);

            switch (m_TextureFormat)
            {
                case TextureFormat.Alpha8: //Alpha8
                    {
                        dwFlags2 = 0x2;
                        dwRGBBitCount = 0x8;
                        dwRBitMask = 0x0;
                        dwGBitMask = 0x0;
                        dwBBitMask = 0x0;
                        dwABitMask = 0xFF;
                        break;
                    }
                case TextureFormat.ARGB4444: //A4R4G4B4
                    {
                        if (reader.platform == BuildTarget.XBOX360) //swap bytes for Xbox confirmed, PS3 not encountered
                        {
                            for (int i = 0; i < (image_data_size / 2); i++)
                            {
                                byte b0 = image_data_bytes[i * 2];
                                image_data_bytes[i * 2] = image_data_bytes[i * 2 + 1];
                                image_data_bytes[i * 2 + 1] = b0;
                            }
                        }
                        else if (reader.platform == BuildTarget.Android) //swap bits for android
                        {
                            for (int i = 0; i < (image_data_size / 2); i++)
                            {
                                byte[] argb = BitConverter.GetBytes((BitConverter.ToInt32((new byte[4] { image_data_bytes[i * 2], image_data_bytes[i * 2 + 1], image_data_bytes[i * 2], image_data_bytes[i * 2 + 1] }), 0)) >> 4);
                                image_data_bytes[i * 2] = argb[0];
                                image_data_bytes[i * 2 + 1] = argb[1];
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
                case TextureFormat.RGB24:
                case TextureFormat.BGR24: //B8G8R8 //confirmed on X360, iOS //PS3 unsure
                    {
                        for (int i = 0; i < (image_data_size / 3); i++)
                        {
                            byte b0 = image_data_bytes[i * 3];
                            image_data_bytes[i * 3] = image_data_bytes[i * 3 + 2];
                            //image_data[i * 3 + 1] stays the same
                            image_data_bytes[i * 3 + 2] = b0;

                        }

                        dwFlags2 = 0x40;
                        dwRGBBitCount = 0x18;
                        dwRBitMask = 0xFF0000;
                        dwGBitMask = 0xFF00;
                        dwBBitMask = 0xFF;
                        dwABitMask = 0x0;
                        break;
                    }
                case TextureFormat.RGBA32: //G8R8A8B8 //confirmed on X360, iOS
                    {
                        for (int i = 0; i < (image_data_size / 4); i++)
                        {
                            byte b0 = image_data_bytes[i * 4];
                            image_data_bytes[i * 4] = image_data_bytes[i * 4 + 2];
                            //image_data[i * 4 + 1] stays the same
                            image_data_bytes[i * 4 + 2] = b0;
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
                case TextureFormat.BGRA32: //B8G8R8A8 //confirmed on X360, PS3, Web, iOS
                    {
                        for (int i = 0; i < (image_data_size / 4); i++)
                        {
                            byte b0 = image_data_bytes[i * 4];
                            byte b1 = image_data_bytes[i * 4 + 1];
                            image_data_bytes[i * 4] = image_data_bytes[i * 4 + 3];
                            image_data_bytes[i * 4 + 1] = image_data_bytes[i * 4 + 2];
                            image_data_bytes[i * 4 + 2] = b1;
                            image_data_bytes[i * 4 + 3] = b0;

                        }

                        dwFlags2 = 0x41;
                        dwRGBBitCount = 0x20;
                        dwRBitMask = 0xFF0000;
                        dwGBitMask = 0xFF00;
                        dwBBitMask = 0xFF;
                        dwABitMask = -16777216;
                        break;
                    }
                case TextureFormat.RGB565: //R5G6B5 //confirmed switched on X360; confirmed on iOS
                    {
                        if (reader.platform == BuildTarget.XBOX360)
                        {
                            for (int i = 0; i < (image_data_size / 2); i++)
                            {
                                byte b0 = image_data_bytes[i * 2];
                                image_data_bytes[i * 2] = image_data_bytes[i * 2 + 1];
                                image_data_bytes[i * 2 + 1] = b0;
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
                case TextureFormat.DXT1: //DXT1
                    {
                        if (reader.platform == BuildTarget.XBOX360) //X360 only, PS3 not
                        {
                            for (int i = 0; i < (image_data_size / 2); i++)
                            {
                                byte b0 = image_data_bytes[i * 2];
                                image_data_bytes[i * 2] = image_data_bytes[i * 2 + 1];
                                image_data_bytes[i * 2 + 1] = b0;
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
                case TextureFormat.DXT5: //DXT5
                    {
                        if (reader.platform == BuildTarget.XBOX360) //X360, PS3 not
                        {
                            for (int i = 0; i < (image_data_size / 2); i++)
                            {
                                byte b0 = image_data_bytes[i * 2];
                                image_data_bytes[i * 2] = image_data_bytes[i * 2 + 1];
                                image_data_bytes[i * 2 + 1] = b0;
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
                case TextureFormat.RGBA4444: //R4G4B4A4, iOS (only?)
                    {
                        for (int i = 0; i < (image_data_size / 2); i++)
                        {
                            byte[] argb = BitConverter.GetBytes((BitConverter.ToInt32((new byte[4] { image_data_bytes[i * 2], image_data_bytes[i * 2 + 1], image_data_bytes[i * 2], image_data_bytes[i * 2 + 1] }), 0)) >> 4);
                            image_data_bytes[i * 2] = argb[0];
                            image_data_bytes[i * 2 + 1] = argb[1];
                        }

                        dwFlags2 = 0x41;
                        dwRGBBitCount = 0x10;
                        dwRBitMask = 0xF00;
                        dwGBitMask = 0xF0;
                        dwBBitMask = 0xF;
                        dwABitMask = 0xF000;
                        break;
                    }
                case TextureFormat.DXT1Crunched: //DXT1 Crunched
                case TextureFormat.DXT5Crunched: //DXT1 Crunched
                    break;
                case TextureFormat.PVRTC_RGB2: //PVRTC_RGB2
                    {
                        pvrPixelFormat = 0x0;
                        break;
                    }
                case TextureFormat.PVRTC_RGBA2: //PVRTC_RGBA2
                    {
                        pvrPixelFormat = 0x1;
                        break;
                    }
                case TextureFormat.PVRTC_RGB4: //PVRTC_RGB4
                    {
                        pvrPixelFormat = 0x2;
                        break;
                    }
                case TextureFormat.PVRTC_RGBA4: //PVRTC_RGBA4
                    {
                        pvrPixelFormat = 0x3;
                        break;
                    }
                case TextureFormat.ETC_RGB4: //ETC_RGB4
                case TextureFormat.ETC2_RGB:
                    {
                        pvrPixelFormat = 0x16;
                        break;
                    }
                case TextureFormat.ETC2_RGBA8: //ETC2_RGBA8
                    {
                        pvrPixelFormat = 0x17;
                        break;
                    }
            }
                    }
            }

    public enum TextureFormat
    {
        Alpha8 = 1,
        ARGB4444,
        RGB24,
        RGBA32,
        ARGB32,
        ARGBFloat,
        RGB565,
        BGR24,
        R16,
        DXT1,
        DXT3,
        DXT5,
        RGBA4444,
        BGRA32,
        RHalf,
        RGHalf,
        RGBAHalf,
        RFloat,
        RGFloat,
        RGBAFloat,
        YUY2,
        RGB9e5Float,
        RGBFloat,
        BC6H,
        BC7,
        BC4,
        BC5,
        DXT1Crunched,
        DXT5Crunched,
        PVRTC_RGB2,
        PVRTC_RGBA2,
        PVRTC_RGB4,
        PVRTC_RGBA4,
        ETC_RGB4,
        ATC_RGB4,
        ATC_RGBA8,
        EAC_R = 41,
        EAC_R_SIGNED,
        EAC_RG,
        EAC_RG_SIGNED,
        ETC2_RGB,
        ETC2_RGBA1,
        ETC2_RGBA8,
        ASTC_RGB_4x4,
        ASTC_RGB_5x5,
        ASTC_RGB_6x6,
        ASTC_RGB_8x8,
        ASTC_RGB_10x10,
        ASTC_RGB_12x12,
        ASTC_RGBA_4x4,
        ASTC_RGBA_5x5,
        ASTC_RGBA_6x6,
        ASTC_RGBA_8x8,
        ASTC_RGBA_10x10,
        ASTC_RGBA_12x12,
        ETC_RGB4_3DS,
        ETC_RGBA8_3DS,
        RG16,
        R8,
        ETC_RGB4Crunched,
        ETC2_RGBA8Crunched,
        ASTC_HDR_4x4,
        ASTC_HDR_5x5,
        ASTC_HDR_6x6,
        ASTC_HDR_8x8,
        ASTC_HDR_10x10,
        ASTC_HDR_12x12,
        RG32,
        RGB48,
        RGBA64
    }
}