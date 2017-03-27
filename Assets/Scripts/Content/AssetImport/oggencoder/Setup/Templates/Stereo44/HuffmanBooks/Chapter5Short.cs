namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter5Short : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            5, 8, 10, 14, 11, 11, 12, 16, 15, 17, 5, 5, 7, 9, 7, 8,
            10, 13, 17, 17, 7, 5, 5, 10, 5, 7, 8, 11, 13, 15, 10, 8,
            10, 8, 8, 8, 11, 15, 18, 18, 8, 5, 5, 8, 3, 4, 6, 10,
            14, 16, 9, 7, 6, 7, 4, 3, 5, 9, 14, 18, 10, 9, 8, 10,
            6, 5, 6, 9, 14, 18, 12, 12, 11, 12, 8, 7, 8, 11, 14, 18,
            14, 13, 12, 10, 7, 5, 6, 9, 14, 18, 14, 14, 13, 10, 6, 5,
            6, 8, 11, 16
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}