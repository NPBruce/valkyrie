namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter2Long : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            6, 6, 12, 10, 10, 10, 9, 10, 12, 12, 6, 1, 10, 5, 6, 6,
            7, 9, 11, 14, 12, 9, 8, 11, 7, 8, 9, 11, 13, 15, 10, 5,
            12, 7, 8, 7, 9, 12, 14, 15, 10, 6, 7, 8, 5, 6, 7, 9,
            12, 14, 9, 6, 8, 7, 6, 6, 7, 9, 12, 12, 9, 7, 9, 9,
            7, 6, 6, 7, 10, 10, 10, 9, 10, 11, 8, 7, 6, 6, 8, 10,
            12, 11, 13, 13, 11, 10, 8, 8, 8, 10, 11, 13, 15, 15, 14, 13,
            10, 8, 8, 9
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}