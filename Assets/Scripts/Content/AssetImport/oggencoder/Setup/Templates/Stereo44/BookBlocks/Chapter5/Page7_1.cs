namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter5
{
    public class Page7_1 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            2, 4, 4, 6, 6, 7, 7, 8, 8, 8, 8, 10, 5, 5, 6, 6,
            7, 7, 8, 8, 8, 8, 10, 5, 5, 6, 6, 7, 7, 8, 8, 8,
            8, 10, 6, 6, 7, 7, 8, 8, 8, 8, 8, 8, 10, 10, 10, 7,
            7, 8, 8, 8, 8, 8, 8, 10, 10, 10, 7, 7, 8, 8, 8, 8,
            8, 8, 10, 10, 10, 7, 7, 8, 8, 8, 8, 8, 8, 10, 10, 10,
            8, 8, 8, 8, 8, 8, 8, 9, 10, 10, 10, 10, 10, 8, 8, 8,
            8, 8, 8, 10, 10, 10, 10, 10, 9, 9, 8, 8, 8, 8, 10, 10,
            10, 10, 10, 8, 8, 8, 8, 8, 8
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