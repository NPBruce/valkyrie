namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter7Short : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            4, 11, 12, 14, 15, 15, 17, 17, 18, 18, 5, 6, 6, 8, 9, 10,
            13, 17, 18, 19, 7, 5, 4, 6, 8, 9, 11, 15, 19, 19, 8, 6,
            5, 5, 6, 7, 11, 14, 16, 17, 9, 7, 7, 6, 7, 7, 10, 13,
            15, 19, 10, 8, 7, 6, 7, 6, 7, 9, 14, 16, 12, 10, 9, 7,
            7, 6, 4, 5, 10, 15, 14, 13, 11, 7, 6, 6, 4, 2, 7, 13,
            16, 16, 15, 9, 8, 8, 8, 6, 9, 13, 19, 19, 17, 12, 11, 10,
            10, 9, 11, 14
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}