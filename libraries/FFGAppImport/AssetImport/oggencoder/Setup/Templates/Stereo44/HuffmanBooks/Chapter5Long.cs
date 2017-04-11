namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter5Long : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            3, 8, 9, 13, 10, 12, 12, 12, 12, 12, 6, 4, 6, 8, 6, 8,
            10, 10, 11, 12, 8, 5, 4, 10, 4, 7, 8, 9, 10, 11, 13, 8,
            10, 8, 9, 9, 11, 12, 13, 14, 10, 6, 4, 9, 3, 5, 6, 8,
            10, 11, 11, 8, 6, 9, 5, 5, 6, 7, 9, 11, 12, 9, 7, 11,
            6, 6, 6, 7, 8, 10, 12, 11, 9, 12, 7, 7, 6, 6, 7, 9,
            13, 12, 10, 13, 9, 8, 7, 7, 7, 8, 11, 15, 11, 15, 11, 10,
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