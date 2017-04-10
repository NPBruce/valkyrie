namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter1ShortManaged : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            4, 7, 13, 14, 14, 15, 16, 18, 18, 4, 2, 5, 8, 7, 9, 12,
            15, 15, 10, 4, 5, 10, 6, 8, 11, 15, 17, 12, 5, 7, 5, 6,
            8, 11, 14, 17, 11, 5, 6, 6, 5, 6, 9, 13, 17, 12, 6, 7,
            6, 5, 6, 8, 12, 14, 14, 7, 8, 6, 6, 7, 9, 11, 14, 14,
            8, 9, 6, 5, 6, 9, 11, 13, 16, 10, 10, 7, 6, 7, 8, 10,
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