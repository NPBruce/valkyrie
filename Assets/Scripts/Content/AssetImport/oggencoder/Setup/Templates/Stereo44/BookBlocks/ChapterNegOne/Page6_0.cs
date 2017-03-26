namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ChapterNegOne
{
    public class Page6_0 : IStaticCodeBook
    {
        public int Dimensions = 4;

        public byte[] LengthList = {
            1, 4, 4, 7, 6, 6, 7, 6, 6, 4, 6, 6, 10, 9, 9, 11,
            9, 9, 4, 6, 6, 10, 9, 9, 10, 9, 9, 7, 10, 10, 11, 11,
            11, 12, 11, 11, 7, 9, 9, 11, 11, 10, 11, 10, 10, 7, 9, 9,
            11, 10, 11, 11, 10, 10, 7, 10, 10, 11, 11, 11, 12, 11, 11, 7,
            9, 9, 11, 10, 10, 11, 10, 10, 7, 9, 9, 11, 10, 10, 11, 10,
            10
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -529137664;
        public int QuantDelta = 1618345984;
        public int Quant = 2;
        public int QuantSequenceP = 0;

        public int[] QuantList = {
            1,
            0,
            2
        };
    }
}