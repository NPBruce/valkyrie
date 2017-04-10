namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class ChapterNegOneShortManaged : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            5, 6, 12, 14, 12, 14, 16, 17, 18, 4, 2, 5, 11, 7, 10, 12,
            14, 15, 9, 4, 5, 11, 7, 10, 13, 15, 18, 15, 6, 7, 5, 6,
            8, 11, 13, 16, 11, 5, 6, 5, 5, 6, 9, 13, 15, 12, 5, 7,
            6, 5, 6, 9, 12, 14, 12, 6, 7, 8, 6, 7, 9, 12, 13, 14,
            8, 8, 7, 5, 5, 8, 10, 12, 16, 9, 9, 8, 6, 6, 7, 9,
            9
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}