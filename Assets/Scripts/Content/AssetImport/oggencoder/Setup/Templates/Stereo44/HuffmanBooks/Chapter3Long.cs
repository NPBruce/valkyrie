namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter3Long : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            5, 6, 11, 11, 11, 11, 10, 10, 12, 11, 5, 2, 11, 5, 6, 6,
            7, 9, 11, 13, 13, 10, 7, 11, 6, 7, 8, 9, 10, 12, 11, 5,
            11, 6, 8, 7, 9, 11, 14, 15, 11, 6, 6, 8, 4, 5, 7, 8,
            10, 13, 10, 5, 7, 7, 5, 5, 6, 8, 10, 11, 10, 7, 7, 8,
            6, 5, 5, 7, 9, 9, 11, 8, 8, 11, 8, 7, 6, 6, 7, 9,
            12, 11, 10, 13, 9, 9, 7, 7, 7, 9, 11, 13, 12, 15, 12, 11,
            9, 8, 8, 8
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}