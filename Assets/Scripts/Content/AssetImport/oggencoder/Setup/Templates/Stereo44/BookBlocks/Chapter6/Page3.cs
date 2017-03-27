namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter6
{
    public class Page3 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            2, 3, 4, 6, 6, 7, 7, 9, 9, 0, 4, 4, 6, 6, 7, 7,
            9, 10, 0, 4, 4, 6, 6, 7, 7, 10, 9, 0, 5, 5, 7, 7,
            8, 8, 10, 10, 0, 0, 0, 7, 6, 8, 8, 10, 10, 0, 0, 0,
            7, 7, 9, 9, 11, 11, 0, 0, 0, 7, 7, 9, 9, 11, 11, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -531628032;
        public int QuantDelta = 1611661312;
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