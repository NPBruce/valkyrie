namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class ChapterNegOneLongManaged : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            3, 3, 8, 8, 8, 8, 10, 12, 14, 3, 2, 6, 7, 7, 8, 10,
            12, 16, 7, 6, 7, 9, 8, 10, 12, 14, 16, 8, 6, 8, 4, 5,
            7, 9, 11, 13, 7, 6, 8, 5, 6, 7, 9, 11, 14, 8, 8, 10,
            7, 7, 6, 8, 10, 13, 9, 11, 12, 9, 9, 7, 8, 10, 12, 10,
            13, 15, 11, 11, 10, 9, 10, 13, 13, 16, 17, 14, 15, 14, 13, 14,
            17
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}