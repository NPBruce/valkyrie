namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter2Short : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            11, 9, 13, 12, 12, 11, 12, 12, 13, 15, 8, 2, 11, 4, 8, 5,
            7, 10, 12, 15, 13, 7, 10, 9, 8, 8, 10, 13, 17, 17, 11, 4,
            12, 5, 9, 5, 8, 11, 14, 16, 12, 6, 8, 7, 6, 6, 8, 11,
            13, 16, 11, 4, 9, 5, 6, 4, 6, 10, 13, 16, 11, 6, 11, 7,
            7, 6, 7, 10, 13, 15, 13, 9, 12, 9, 8, 6, 8, 10, 12, 14,
            14, 10, 10, 8, 6, 5, 6, 9, 11, 13, 15, 11, 11, 9, 6, 5,
            6, 8, 9, 12
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}