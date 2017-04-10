namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter0
{
    public class Page8_1 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 4, 4, 6, 6, 7, 7, 9, 9, 11, 12, 13, 12, 6, 5, 5,
            7, 7, 8, 8, 10, 9, 12, 12, 12, 12, 6, 5, 5, 7, 7, 8,
            8, 10, 9, 12, 11, 11, 13, 16, 7, 7, 8, 8, 9, 9, 10, 10,
            12, 12, 13, 12, 16, 7, 7, 8, 7, 9, 9, 10, 10, 11, 12, 12,
            13, 16, 10, 10, 8, 8, 10, 10, 11, 12, 12, 12, 13, 13, 16, 11,
            10, 8, 7, 11, 10, 11, 11, 12, 11, 13, 13, 16, 16, 16, 10, 10,
            10, 10, 11, 11, 13, 12, 13, 13, 16, 16, 16, 11, 9, 11, 9, 15,
            13, 12, 13, 13, 13, 16, 16, 16, 15, 13, 11, 11, 12, 13, 12, 12,
            14, 13, 16, 16, 16, 14, 13, 11, 11, 13, 12, 14, 13, 13, 13, 16,
            16, 16, 16, 16, 13, 13, 13, 12, 14, 13, 14, 14, 16, 16, 16, 16,
            16, 13, 13, 12, 12, 14, 14, 15, 13
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