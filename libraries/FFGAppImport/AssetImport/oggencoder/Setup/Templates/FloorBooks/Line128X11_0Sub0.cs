namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X11_0Sub0 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            5, 5, 5, 5, 5, 5, 6, 5, 6, 5, 6, 5, 6, 5, 6, 5,
            6, 5, 6, 5, 6, 5, 6, 5, 6, 5, 6, 6, 6, 6, 7, 6,
            7, 6, 7, 6, 7, 6, 7, 6, 7, 6, 8, 6, 8, 6, 8, 7,
            8, 7, 8, 7, 8, 7, 9, 7, 9, 8, 9, 8, 9, 8, 10, 8,
            10, 9, 10, 9, 10, 9, 11, 9, 11, 9, 10, 10, 11, 10, 11, 10,
            11, 11, 11, 11, 11, 11, 12, 13, 14, 14, 14, 15, 15, 16, 16, 16,
            17, 15, 16, 15, 16, 16, 17, 17, 16, 17, 17, 17, 17, 17, 17, 17,
            17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}