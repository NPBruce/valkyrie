namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter8
{
    public class Page6_1 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            3, 4, 4, 5, 5, 5, 4, 4, 5, 5, 5, 4, 4, 5, 5, 6,
            5, 5, 5, 5, 6, 6, 6, 5, 5
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -533725184;
        public int QuantDelta = 1611661312;
        public int Quant = 3;
        public int QuantSequenceP = 0;

        public int[] QuantList = {
            2,
            1,
            3,
            0,
            4
        };
    }
}