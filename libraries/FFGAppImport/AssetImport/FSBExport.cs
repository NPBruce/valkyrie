using System;
using System.Collections.Generic;
using System.IO;
using AssetStudio;
using OggVorbisEncoder;

class FSBExport
{
    public static void Write(byte[] data, string file)
    {
        EndianBinaryReader stream = new EndianBinaryReader(new System.IO.MemoryStream(data), EndianType.LittleEndian);

        // Because constructor is broken
        stream.Endian = EndianType.LittleEndian;

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
            Ogg ogg = new Ogg();
            uint offset = stream.ReadUInt32();
            bool extraHeaders = (offset & 0x01) != 0;
            uint type = offset & ((1 << 7) - 1);
            offset = (offset >> 7) * 0x20;
            ogg.channels = (type >> 5) + 1;
            ogg.frequency = 44100;
            switch ((type >> 1) & ((1 << 4) - 1))
            {
                case 0: ogg.frequency = 4000; break;
                case 1: ogg.frequency = 8000; break;
                case 2: ogg.frequency = 11000; break;
                case 3: ogg.frequency = 12000; break;
                case 4: ogg.frequency = 16000; break;
                case 5: ogg.frequency = 22050; break;
                case 6: ogg.frequency = 24000; break;
                case 7: ogg.frequency = 32000; break;
                case 8: ogg.frequency = 44100; break;
                case 9: ogg.frequency = 48000; break;
                case 10: ogg.frequency = 96000; break;
                default: ogg.frequency = 44100; break;
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
                    ogg.channels = stream.ReadByte();
                    extraLen -= 1;
                }
                if (dataByte == 0x04)
                {
                    ogg.frequency = stream.ReadUInt32();
                    extraLen -= 4;
                }
                if (dataByte == 0x06)
                {
                    ogg.loopStart = stream.ReadUInt32();
                    ogg.loopEnd = stream.ReadUInt32();
                    extraLen -= 8;
                }
                if (dataByte == 0x16)
                {
                    ogg.crc32 = stream.ReadUInt32();
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
                WriteFile(stream, file, (int)fileOffset, (int)size, ogg);
            }
            else
            {
                WriteFile(stream, file + i, (int)fileOffset, (int)size, ogg);
            }

            stream.Position = nextFilePos;
        }
        stream.Dispose(true);
    }

    public static void WriteFile(EndianBinaryReader stream, string file, int offset, int size, Ogg ogg)
    {
        // Write to disk
        using (BinaryWriter writer = new BinaryWriter(File.Open(file, FileMode.Create)))
        {
            // Only support header CRC 3605052372 for now
            OggVorbisHeader head = new OggVorbisHeader();

            HeaderPacketBuilder hpb = new HeaderPacketBuilder();
            CodecSetup cSetup = new CodecSetup(null);
            cSetup.BlockSizes[0] = 256;
            cSetup.BlockSizes[1] = 2048;

            VorbisInfo info = new VorbisInfo(cSetup, (int)ogg.channels, (int)ogg.frequency, 0);

            OggPacket headerInfo = hpb.BuildInfoPacket(info);

            Comments comments = new Comments();
            if (ogg.loopStart > 0 && ogg.loopEnd > 0)
            {
                comments.AddTag("LOOP_START", ogg.loopStart.ToString());
                comments.AddTag("LOOP_END", ogg.loopEnd.ToString());
            }
            OggPacket headerComment = hpb.BuildCommentsPacket(comments);
            OggPacket headerSetup = new OggPacket(OggVorbisHeader.GetHeader(ogg.crc32), false, 0, 2);

            OggStream output = new OggStream(1);
            output.PacketIn(headerInfo);
            output.PacketIn(headerComment);
            output.PacketIn(headerSetup);

            stream.Position = offset;

            UInt16 packetSize = stream.ReadUInt16();
            int prevPacketNo = 2;
            int GranulePos = 0;
            int prevSamples = 0;

            while (packetSize > 0)
            {
                OggPacket packet = new OggPacket(stream.ReadBytes(packetSize), false, 0, prevPacketNo + 1);

                byte firstByte = packet.PacketData[0];

                // OK for stereo
                int noSamples = 256;
                if ((firstByte & 2) != 0)
                {
                    noSamples = 2048;
                }

                if (prevSamples != 0)
                {
                    GranulePos += (prevSamples + noSamples) / 4;
                }
                packet.GranulePosition = GranulePos;
                prevSamples = noSamples;

                if (stream.Position + 2 < offset + size)
                {
                    packetSize = stream.ReadUInt16();
                }
                else
                {
                    packetSize = 0;
                }
                packet.EndOfStream = packetSize == 0;
                prevPacketNo = packet.PacketNumber;

                output.PacketIn(packet);
                OggPage page = null;
                if (output.PageOut(out page, true))
                {
                    writer.Write(page.Header);
                    writer.Write(page.Body);
                }
            }

            //float vorbis_quality = ((ogg.quality - 1) + (ogg.quality - 100) * 0.1f) / 99.0f;
            //VorbisInfo.InitVariableBitRate(ogg.channels, ogg.frequency, ogg.)
            //writer.Write();
            writer.Close();
        }
    }

    public static void RebuildHeader(Ogg ogg)
    {
        // out id comment setup
    }

    public static Header[] GenerateHeaders()
    {
        int[] rates = { 8000, 11000, 16000, 22050, 24000, 32000, 44100, 48000 };
        Dictionary<uint, Header> dict = new Dictionary<uint, Header>();

        for (int quality = 1; quality <= 100; ++quality)
        {
            for (int channels = 1; channels <= 2; ++channels)
            {
                foreach (int rate in rates)
                {
                    Header h = new Header(channels, rate, quality);
                }
            }
        }
        return null;
    }

    public class Header
    {
        //public uint crc32;
        //public int blocksize_short;
        //public int blocksize_long;
        //public uint setup_header_size;
        //public char[] setup_header;

        public Header(int channels, int rate, int quality)
        {
            float vorbis_quality = ((quality - 1) + (quality - 100) * 0.1f) / 99.0f;
            VorbisInfo info = VorbisInfo.InitVariableBitRate(channels, rate, vorbis_quality);
            // missing OV_ECTL_COUPLING_SET to 1, is this needed?

        }
    }

    public class Ogg
    {
        public uint frequency = 0;
        public uint channels = 0;
        public uint loopStart = 0;
        public uint loopEnd = 0;
        public uint crc32 = 0;
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
