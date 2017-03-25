using System;
using System.Collections.Generic;
using System.IO;

class FSBExport
{
    public static void Write(byte[] data, string file)
    {
        Unity_Studio.EndianStream stream = new Unity_Studio.EndianStream(new System.IO.MemoryStream(data), Unity_Studio.EndianType.LittleEndian);

        // Because constructor is broken
        stream.endian = Unity_Studio.EndianType.LittleEndian;

        string magic = stream.ReadASCII(4);

        if (!magic.Equals("FSB5")) return;

        uint version = stream.ReadUInt32();
        uint numSamples = stream.ReadUInt32();
        uint sampleHeadersSize = stream.ReadUInt32();
        uint nameTableSize = stream.ReadUInt32();
        uint dataSize = stream.ReadUInt32();
        uint mode = stream.ReadUInt32();

        string zero = stream.ReadASCII(8);
        string hash = stream.ReadASCII(16);
        string dummy = stream.ReadASCII(8);

        long nameOffset = stream.Position + sampleHeadersSize;
        long baseOffset = nameOffset + nameTableSize;

        // Only support Vorbis
        if (mode != (int) FSBAudioFormat.FMOD_SOUND_FORMAT_VORBIS)
        {
            return;
        }

        for (int i = 0; i < numSamples; i++)
        {
            uint offset = stream.ReadUInt32();
            bool extraHeaders = (offset & 0x01) != 0;
            uint type = offset & ((1 << 7) - 1);
            offset = (offset >> 7) * 0x20;
            uint channels = (type >> 5) + 1;
            uint frequency = 44100;
            switch ((type >> 1) & ((1 << 4) - 1))
            {
                case 0: frequency = 4000; break;
                case 1: frequency = 8000; break;
                case 2: frequency = 11000; break;
                case 3: frequency = 12000; break;
                case 4: frequency = 16000; break;
                case 5: frequency = 22050; break;
                case 6: frequency = 24000; break;
                case 7: frequency = 32000; break;
                case 8: frequency = 44100; break;
                case 9: frequency = 48000; break;
                case 10: frequency = 96000; break;
                default: frequency = 44100; break;
            }

            uint unknown = stream.ReadUInt32() >> 2;

            while (extraHeaders)
            {
                byte dataByte = stream.ReadByte();
                extraHeaders = (dataByte & 0x01) != 0;
                long extraLen = dataByte >> 1;
                extraLen += stream.ReadByte() << 7;
                extraLen += stream.ReadByte() << 15;
                dataByte = stream.ReadByte();
                if (dataByte == 0x02)
                {
                    channels = stream.ReadByte();
                    extraLen -= 1;
                }
                if (dataByte == 0x04)
                {
                    frequency = stream.ReadUInt32();
                    extraLen -= 4;
                }
                if (dataByte == 0x06)
                {
                    uint loopStart = stream.ReadUInt32();
                    uint loopEnd = stream.ReadUInt32();
                    extraLen -= 8;
                }
                if (dataByte == 0x16)
                {
                    uint crc32 = stream.ReadUInt32();
                    extraLen -= 4;
                }
                stream.Position += extraLen;
            }

            long nextFilePos = stream.Position;

            long size = stream.ReadUInt32();
            if (size == 0)
            {
                size = dataSize + baseOffset;
            }
            else
            {
                size = ((size >> 7) *0x20) + baseOffset;
            }
            if (size < 0 || size > data.Length)
            {
                size = data.Length;
            }
            long fileOffset = baseOffset + offset;
            size -= fileOffset;

            if (i == 0)
            {
                WriteFile(data, file, (int)fileOffset, (int)size);
            }
            else
            {
                WriteFile(data, file + i, (int)fileOffset, (int)size);
            }

            stream.Position = nextFilePos;
        }
        stream.Dispose(true);
    }

    public static void WriteFile(byte[] data, string file, int offset, int size)
    {
        // Write to disk
        using (BinaryWriter writer = new BinaryWriter(File.Open(file, FileMode.Create)))
        {
            // Pass the Deobfuscate key to decrypt
            writer.Write(data, offset, size);
            writer.Close();
        }
    }

    public enum FSBAudioFormat
    {
        FMOD_SOUND_FORMAT_NONE,             /* Unitialized / unknown. */
        FMOD_SOUND_FORMAT_PCM8,             /* 8bit integer PCM data. */
        FMOD_SOUND_FORMAT_PCM16,            /* 16bit integer PCM data. */
        FMOD_SOUND_FORMAT_PCM24,            /* 24bit integer PCM data. */
        FMOD_SOUND_FORMAT_PCM32,            /* 32bit integer PCM data. */
        FMOD_SOUND_FORMAT_PCMFLOAT,         /* 32bit floating point PCM data. */
        FMOD_SOUND_FORMAT_GCADPCM,          /* Compressed Nintendo 3DS/Wii DSP data. */
        FMOD_SOUND_FORMAT_IMAADPCM,         /* Compressed IMA ADPCM data. */
        FMOD_SOUND_FORMAT_VAG,              /* Compressed PlayStation Portable ADPCM data. */
        FMOD_SOUND_FORMAT_HEVAG,            /* Compressed PSVita ADPCM data. */
        FMOD_SOUND_FORMAT_XMA,              /* Compressed Xbox360 XMA data. */
        FMOD_SOUND_FORMAT_MPEG,             /* Compressed MPEG layer 2 or 3 data. */
        FMOD_SOUND_FORMAT_CELT,             /* Compressed CELT data. */
        FMOD_SOUND_FORMAT_AT9,              /* Compressed PSVita ATRAC9 data. */
        FMOD_SOUND_FORMAT_XWMA,             /* Compressed Xbox360 xWMA data. */
        FMOD_SOUND_FORMAT_VORBIS,           /* Compressed Vorbis data. */
    }
}
