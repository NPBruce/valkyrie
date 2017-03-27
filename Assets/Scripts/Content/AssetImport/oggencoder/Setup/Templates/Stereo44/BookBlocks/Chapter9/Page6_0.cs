namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter9
{
    public class Page6_0 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            2, 4, 4, 6, 6, 7, 7, 7, 7, 8, 8, 9, 9, 5, 4, 4,
            6, 6, 8, 8, 9, 9, 9, 9, 10, 10, 6, 4, 4, 6, 6, 8,
            8, 9, 9, 9, 9, 10, 10, 0, 6, 6, 7, 7, 8, 8, 9, 9,
            10, 10, 11, 11, 0, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11,
            11, 0, 10, 10, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 0, 11,
            11, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0
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