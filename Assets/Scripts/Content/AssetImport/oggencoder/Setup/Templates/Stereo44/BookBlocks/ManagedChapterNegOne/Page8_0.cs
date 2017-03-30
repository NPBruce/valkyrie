namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ManagedChapterNegOne
{
    public class Page8_0 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 4, 4, 12, 11, 13, 13, 14, 14, 4, 7, 7, 11, 13, 14, 14,
            14, 14, 3, 8, 3, 14, 14, 14, 14, 14, 14, 14, 10, 12, 14, 14,
            14, 14, 14, 14, 14, 14, 5, 14, 8, 14, 14, 14, 14, 14, 12, 14,
            13, 14, 14, 14, 14, 14, 14, 14, 13, 14, 10, 14, 14, 14, 14, 14,
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
            14
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -516186112;
        public int QuantDelta = 1627103232;
        public int Quant = 4;
        public int QuantSequenceP = 0;

        public int[] QuantList = {
            4,
            3,
            5,
            2,
            6,
            1,
            7,
            0,
            8
        };
    }
}