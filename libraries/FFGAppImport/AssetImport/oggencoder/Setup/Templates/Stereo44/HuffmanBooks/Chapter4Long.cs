namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter4Long : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            4, 7, 11, 11, 11, 11, 10, 11, 12, 11, 5, 2, 11, 5, 6, 6,
            7, 9, 11, 12, 11, 9, 6, 10, 6, 7, 8, 9, 10, 11, 11, 5,
            11, 7, 8, 8, 9, 11, 13, 14, 11, 6, 5, 8, 4, 5, 7, 8,
            10, 11, 10, 6, 7, 7, 5, 5, 6, 8, 9, 11, 10, 7, 8, 9,
            6, 6, 6, 7, 8, 9, 11, 9, 9, 11, 7, 7, 6, 6, 7, 9,
            12, 12, 10, 13, 9, 8, 7, 7, 7, 8, 11, 13, 11, 14, 11, 10,
            9, 8, 7, 7
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}