namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line2048X27_0Sub0 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            5, 5, 5, 5, 5, 5, 6, 5, 6, 5, 6, 5, 6, 5, 6, 5,
            6, 5, 7, 5, 7, 5, 7, 5, 8, 5, 8, 5, 8, 5, 9, 5,
            9, 6, 10, 6, 10, 6, 11, 6, 11, 6, 11, 6, 11, 6, 11, 6,
            11, 6, 11, 6, 12, 7, 11, 7, 11, 7, 11, 7, 11, 7, 10, 7,
            11, 7, 11, 7, 12, 7, 11, 8, 11, 8, 11, 8, 11, 8, 13, 8,
            12, 9, 11, 9, 11, 9, 11, 10, 12, 10, 12, 9, 12, 10, 12, 11,
            14, 12, 16, 12, 12, 11, 14, 16, 17, 17, 17, 17, 17, 17, 17, 17,
            17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 16, 16, 16, 16
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}