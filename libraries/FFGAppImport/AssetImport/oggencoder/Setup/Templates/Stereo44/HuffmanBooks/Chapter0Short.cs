namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter0Short : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            9, 8, 12, 11, 12, 13, 14, 14, 16, 6, 1, 5, 6, 6, 9, 12,
            14, 17, 9, 4, 5, 9, 7, 9, 13, 15, 16, 8, 5, 8, 6, 8,
            10, 13, 17, 17, 9, 6, 7, 7, 8, 9, 13, 15, 17, 11, 8, 9,
            9, 9, 10, 12, 16, 16, 13, 7, 8, 7, 7, 9, 12, 14, 15, 13,
            6, 7, 5, 5, 7, 10, 13, 13, 14, 7, 8, 5, 6, 7, 9, 10,
            12
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}