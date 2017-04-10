namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter1Short : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            6, 8, 13, 12, 13, 14, 15, 16, 16, 4, 2, 4, 7, 6, 8, 11,
            13, 15, 10, 4, 4, 8, 6, 8, 11, 14, 17, 11, 5, 6, 5, 6,
            8, 12, 14, 17, 11, 5, 5, 6, 5, 7, 10, 13, 16, 12, 6, 7,
            8, 7, 8, 10, 13, 15, 13, 8, 8, 7, 7, 8, 10, 12, 15, 15,
            7, 7, 5, 5, 7, 9, 12, 14, 15, 8, 8, 6, 6, 7, 8, 10,
            11
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}