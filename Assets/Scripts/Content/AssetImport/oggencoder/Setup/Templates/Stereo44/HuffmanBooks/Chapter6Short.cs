namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter6Short : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            3, 9, 11, 11, 13, 14, 19, 17, 17, 19, 5, 4, 5, 8, 10, 10,
            13, 16, 18, 19, 7, 4, 4, 5, 8, 9, 12, 14, 17, 19, 8, 6,
            5, 5, 7, 7, 10, 13, 16, 18, 10, 8, 7, 6, 5, 5, 8, 11,
            17, 19, 11, 9, 7, 7, 5, 4, 5, 8, 17, 19, 13, 11, 8, 7,
            7, 5, 5, 7, 16, 18, 14, 13, 8, 6, 6, 5, 5, 7, 16, 18,
            18, 16, 10, 8, 8, 7, 7, 9, 16, 18, 18, 18, 12, 10, 10, 9,
            9, 10, 17, 18
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}