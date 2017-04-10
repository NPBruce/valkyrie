namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter0Long : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            5, 4, 8, 9, 8, 9, 10, 12, 15, 4, 1, 5, 5, 6, 8, 11,
            12, 12, 8, 5, 8, 9, 9, 11, 13, 12, 12, 9, 5, 8, 5, 7,
            9, 12, 13, 13, 8, 6, 8, 7, 7, 9, 11, 11, 11, 9, 7, 9,
            7, 7, 7, 7, 10, 12, 10, 10, 11, 9, 8, 7, 7, 9, 11, 11,
            12, 13, 12, 11, 9, 8, 9, 11, 13, 16, 16, 15, 15, 12, 10, 11,
            12
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}