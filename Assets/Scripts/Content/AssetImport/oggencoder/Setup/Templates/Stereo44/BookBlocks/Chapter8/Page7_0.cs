namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter8
{
    public class Page7_0 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 4, 4, 6, 6, 8, 7, 9, 9, 10, 10, 12, 12, 6, 5, 5,
            7, 7, 8, 8, 10, 10, 11, 11, 12, 12, 7, 5, 5, 7, 7, 8,
            8, 10, 10, 11, 11, 12, 12, 21, 7, 7, 7, 7, 8, 9, 10, 10,
            11, 11, 12, 12, 21, 7, 7, 7, 7, 9, 9, 10, 10, 12, 12, 13,
            13, 21, 11, 11, 8, 8, 9, 9, 11, 11, 12, 12, 13, 13, 21, 11,
            11, 8, 8, 9, 9, 11, 11, 12, 12, 13, 13, 21, 21, 21, 10, 10,
            10, 10, 11, 11, 12, 13, 13, 13, 21, 21, 21, 10, 10, 10, 10, 11,
            11, 13, 13, 14, 13, 21, 21, 21, 13, 13, 11, 11, 12, 12, 13, 13,
            14, 14, 21, 21, 21, 14, 14, 11, 11, 12, 12, 13, 13, 14, 14, 21,
            21, 21, 21, 20, 13, 13, 13, 12, 14, 14, 16, 15, 20, 20, 20, 20,
            20, 13, 13, 13, 13, 14, 13, 15, 15
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