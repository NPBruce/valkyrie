namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line2048X27_4Sub2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 3, 2, 4, 3, 4, 4, 4, 5, 5, 6, 5, 6, 5, 7,
            6, 6, 6, 7, 7, 7, 8, 9, 9, 9, 12, 10, 11, 10, 10, 12,
            10, 10
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}