namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter7
{
    public class Page7_0 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 4, 4, 6, 6, 7, 8, 9, 9, 10, 10, 12, 11, 6, 5, 5,
            7, 7, 8, 8, 9, 10, 11, 11, 12, 12, 7, 5, 5, 7, 7, 8,
            8, 10, 10, 11, 11, 12, 12, 20, 7, 7, 7, 7, 8, 9, 10, 10,
            11, 11, 12, 13, 20, 7, 7, 7, 7, 9, 9, 10, 10, 11, 12, 13,
            13, 20, 11, 11, 8, 8, 9, 9, 11, 11, 12, 12, 13, 13, 20, 11,
            11, 8, 8, 9, 9, 11, 11, 12, 12, 13, 13, 20, 20, 20, 10, 10,
            10, 10, 12, 12, 13, 13, 13, 13, 20, 20, 20, 10, 10, 10, 10, 12,
            12, 13, 13, 13, 14, 20, 20, 20, 14, 14, 11, 11, 12, 12, 13, 13,
            14, 14, 20, 20, 20, 14, 14, 11, 11, 12, 12, 13, 13, 14, 14, 20,
            20, 20, 20, 19, 13, 13, 13, 13, 14, 14, 15, 14, 19, 19, 19, 19,
            19, 13, 13, 13, 13, 14, 14, 15, 15
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -523206656;
        public int QuantDelta = 1618345984;
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