using System;
using System.IO;
using SevenZip.Compression.LZMA;


namespace AssetStudio
{
    public static class SevenZipHelper
    {
        public static MemoryStream StreamDecompress(MemoryStream inStream)
        {
            var decoder = new Decoder();

            inStream.Seek(0, SeekOrigin.Begin);
            var newOutStream = new MemoryStream();

            var properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            long outSize = 0;
            for (var i = 0; i < 8; i++)
            {
                var v = inStream.ReadByte();
                if (v < 0)
                    throw new Exception("Can't Read 1");
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties);

            var compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, newOutStream, compressedSize, outSize, new LzmaProgress(outSize));

            newOutStream.Position = 0;
            return newOutStream;
        }

        public static void StreamDecompress(Stream compressedStream, Stream decompressedStream, long compressedSize, long decompressedSize)
        {
            var basePosition = compressedStream.Position;
            var decoder = new Decoder();
            var properties = new byte[5];
            if (compressedStream.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            decoder.SetDecoderProperties(properties);
            decoder.Code(compressedStream, decompressedStream, compressedSize - 5, decompressedSize, new LzmaProgress(decompressedSize));
            compressedStream.Position = basePosition + compressedSize;
        }

        private class LzmaProgress : SevenZip.ICodeProgress
        {
            private long m_decompressedSize;
            private int lastPercentage = -1;

            public LzmaProgress(long decompressedSize)
            {
                m_decompressedSize = decompressedSize;
            }

            public void SetProgress(Int64 inSize, Int64 outSize)
            {
                if (m_decompressedSize > 0)
                {
                    int percentage10000 = (int)((outSize * 10000) / m_decompressedSize);
                    // To prevent logging thousands of times a second, we only log when the percentage changes by 0.01%
                    if (percentage10000 != lastPercentage)
                    {
                        lastPercentage = percentage10000;
                        float percentage = percentage10000 / 100f;
                        ValkyrieTools.ValkyrieDebug.Log($"Decompressing bundle block ({percentage.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}%)...");
                    }
                }
            }
        }
    }
}
