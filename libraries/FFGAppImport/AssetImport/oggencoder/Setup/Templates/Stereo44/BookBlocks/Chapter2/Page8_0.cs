namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter2
{
    public class Page8_0 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 4, 4, 6, 6, 7, 7, 7, 7, 8, 8, 9, 9, 6, 5, 5,
            7, 7, 8, 8, 8, 8, 9, 9, 10, 10, 7, 6, 5, 7, 7, 8,
            8, 8, 8, 9, 9, 10, 10, 0, 8, 8, 8, 8, 9, 9, 9, 9,
            10, 10, 11, 11, 0, 8, 8, 8, 8, 9, 9, 9, 9, 10, 10, 11,
            11, 0, 12, 12, 9, 9, 10, 10, 10, 10, 11, 11, 11, 11, 0, 13,
            13, 9, 9, 10, 10, 10, 10, 11, 11, 12, 12, 0, 0, 0, 10, 10,
            10, 10, 11, 11, 12, 12, 12, 13, 0, 0, 0, 10, 10, 10, 10, 11,
            11, 12, 12, 12, 12, 0, 0, 0, 14, 14, 10, 11, 11, 11, 12, 12,
            13, 13, 0, 0, 0, 14, 14, 11, 10, 11, 11, 13, 12, 13, 13, 0,
            0, 0, 0, 0, 12, 12, 11, 12, 13, 12, 14, 14, 0, 0, 0, 0,
            0, 12, 12, 12, 12, 13, 12, 14, 14
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -526516224;
        public int QuantDelta = 1616117760;
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