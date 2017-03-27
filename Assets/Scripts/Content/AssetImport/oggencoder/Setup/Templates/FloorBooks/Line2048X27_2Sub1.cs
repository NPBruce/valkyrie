namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line2048X27_2Sub1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            3, 4, 3, 4, 3, 4, 4, 5, 4, 5, 5, 5, 6, 6, 6, 7,
            6, 8, 6, 8, 6, 9, 7, 10, 7, 10, 7, 10, 7, 12, 7, 12,
            7, 12, 9, 12, 11, 12, 10, 12, 10, 12, 11, 12, 12, 12, 10, 12,
            10, 12, 10, 12, 9, 12, 11, 12, 12, 12, 12, 12, 11, 12, 11, 12,
            12, 12, 12, 12, 12, 12, 12, 12, 10, 10, 12, 12, 12, 12, 12, 10,
            12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}