namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.ChapterNegOne
{
    public class Page3 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 2, 3, 7, 7, 0, 0, 0, 0, 0, 0, 0, 6, 6, 0, 0,
            0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 0, 7, 7,
            0, 0, 0, 0, 0, 0, 0, 7, 7, 0, 0, 0, 0, 0, 0, 0,
            9, 8, 0, 0, 0, 0, 0, 0, 0, 8, 8, 0, 0, 0, 0, 0,
            0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -531628032;
        public int QuantDelta = 1611661312;
        public int Quant = 4;
        public int QuantSequenceP = 0;
        public int[] QuantList = {4, 3, 5, 2, 6, 1, 7, 0, 8};
    }
}