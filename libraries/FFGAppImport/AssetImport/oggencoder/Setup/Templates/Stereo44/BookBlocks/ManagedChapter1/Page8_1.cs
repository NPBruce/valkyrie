namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ManagedChapter1
{
    public class Page8_1 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 4, 4, 6, 6, 7, 7, 9, 9, 10, 11, 12, 12, 6, 5, 5,
            7, 7, 8, 7, 10, 10, 11, 11, 12, 12, 6, 5, 5, 7, 7, 8,
            8, 10, 10, 11, 11, 12, 12, 16, 7, 7, 8, 8, 9, 9, 11, 11,
            12, 12, 13, 13, 17, 7, 7, 8, 7, 9, 9, 11, 10, 12, 12, 13,
            13, 19, 11, 10, 8, 8, 10, 10, 11, 11, 12, 12, 13, 13, 19, 11,
            11, 9, 7, 11, 10, 11, 11, 12, 12, 13, 12, 19, 19, 19, 10, 10,
            10, 10, 11, 12, 12, 12, 13, 14, 18, 19, 19, 11, 9, 11, 9, 13,
            12, 12, 12, 13, 13, 19, 20, 19, 13, 15, 11, 11, 12, 12, 13, 13,
            14, 13, 18, 19, 20, 15, 13, 12, 10, 13, 10, 13, 13, 13, 14, 20,
            20, 20, 20, 20, 13, 14, 12, 12, 13, 12, 13, 13, 20, 20, 20, 20,
            20, 13, 12, 12, 12, 14, 12, 14, 13
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