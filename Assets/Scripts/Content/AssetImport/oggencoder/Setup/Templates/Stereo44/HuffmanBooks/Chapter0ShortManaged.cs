namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class Chapter0ShortManaged : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            6, 6, 12, 13, 13, 14, 16, 17, 17, 4, 2, 5, 8, 7, 9, 12,
            15, 15, 9, 4, 5, 9, 7, 9, 12, 16, 18, 11, 6, 7, 4, 6,
            8, 11, 14, 18, 10, 5, 6, 5, 5, 7, 10, 14, 17, 10, 5, 7,
            7, 6, 7, 10, 13, 16, 11, 5, 7, 7, 7, 8, 10, 12, 15, 13,
            6, 7, 5, 5, 7, 9, 12, 13, 16, 8, 9, 6, 6, 7, 9, 10,
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