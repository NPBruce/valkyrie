namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter9
{
    public class Page5_0 : IStaticCodeBook
    {
        public int Dimensions = 4;

        public byte[] LengthList = {
            1, 4, 4, 5, 7, 7, 6, 7, 7, 4, 7, 6, 9, 10, 10, 10,
            10, 9, 4, 6, 7, 9, 10, 10, 10, 9, 10, 5, 9, 9, 9, 11,
            11, 10, 11, 11, 7, 10, 9, 11, 12, 11, 12, 12, 12, 7, 9, 10,
            11, 11, 12, 12, 12, 12, 6, 10, 10, 10, 12, 12, 10, 12, 11, 7,
            10, 10, 11, 12, 12, 11, 12, 12, 7, 10, 10, 11, 12, 12, 12, 12,
            12
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