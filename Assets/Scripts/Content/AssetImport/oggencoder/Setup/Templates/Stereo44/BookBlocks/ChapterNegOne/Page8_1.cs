namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ChapterNegOne
{
    public class Page8_1 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 4, 4, 6, 6, 8, 8, 9, 10, 10, 11, 11, 11, 6, 5, 5,
            7, 7, 8, 8, 9, 10, 9, 11, 11, 12, 5, 5, 5, 7, 7, 8,
            9, 10, 10, 12, 12, 14, 13, 15, 7, 7, 8, 8, 9, 10, 11, 11,
            10, 12, 10, 11, 15, 7, 8, 8, 8, 9, 9, 11, 11, 13, 12, 12,
            13, 15, 10, 10, 8, 8, 10, 10, 12, 12, 11, 14, 10, 10, 15, 11,
            11, 8, 8, 10, 10, 12, 13, 13, 14, 15, 13, 15, 15, 15, 10, 10,
            10, 10, 12, 12, 13, 12, 13, 10, 15, 15, 15, 10, 10, 11, 10, 13,
            11, 13, 13, 15, 13, 15, 15, 15, 13, 13, 10, 11, 11, 11, 12, 10,
            14, 11, 15, 15, 14, 14, 13, 10, 10, 12, 11, 13, 13, 14, 14, 15,
            15, 15, 15, 15, 11, 11, 11, 11, 12, 11, 15, 12, 15, 15, 15, 15,
            15, 12, 12, 11, 11, 14, 12, 13, 14
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -522616832;
        public int QuantDelta = 1620115456;
        public int Quant = 4;
        public int QuantSequenceP = 0;

        public int[] QuantList = {
            6,
            5,
            7,
            4,
            8,
            3,
            9,
            2,
            10,
            1,
            11,
            0,
            12
        };
    }
}