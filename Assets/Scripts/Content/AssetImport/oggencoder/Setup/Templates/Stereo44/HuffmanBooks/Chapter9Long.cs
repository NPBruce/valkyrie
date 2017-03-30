namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter9Long : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            3, 8, 12, 14, 15, 15, 15, 13, 15, 15, 6, 5, 8, 10, 12, 12,
            13, 12, 14, 13, 10, 6, 5, 6, 8, 9, 11, 11, 13, 13, 13, 8,
            5, 4, 5, 6, 8, 10, 11, 13, 14, 10, 7, 5, 4, 5, 7, 9,
            11, 12, 13, 11, 8, 6, 5, 4, 5, 7, 9, 11, 12, 11, 10, 8,
            7, 5, 4, 5, 9, 10, 13, 13, 11, 10, 8, 6, 5, 4, 7, 9,
            15, 14, 13, 12, 10, 9, 8, 7, 8, 9, 12, 12, 14, 13, 12, 11,
            10, 9, 8, 9
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}