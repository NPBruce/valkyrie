namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter8
{
    public class Page5_1 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            3, 5, 5, 6, 6, 7, 7, 8, 8, 8, 8, 11, 4, 5, 6, 6,
            7, 7, 8, 8, 8, 8, 11, 5, 5, 6, 6, 7, 7, 8, 8, 8,
            9, 12, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 12, 12, 12, 6,
            6, 7, 7, 8, 8, 9, 9, 11, 11, 11, 6, 6, 7, 7, 8, 8,
            8, 8, 11, 11, 11, 6, 6, 7, 7, 8, 8, 8, 8, 11, 11, 11,
            7, 7, 7, 8, 8, 8, 8, 8, 11, 11, 11, 11, 11, 7, 7, 8,
            8, 8, 8, 11, 11, 11, 11, 11, 7, 7, 7, 7, 8, 8, 11, 11,
            11, 11, 11, 7, 7, 7, 7, 8, 8
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -531365888;
        public int QuantDelta = 1611661312;
        public int Quant = 4;
        public int QuantSequenceP = 0;

        public int[] QuantList = {
            5,
            4,
            6,
            3,
            7,
            2,
            8,
            1,
            9,
            0,
            10
        };
    }
}