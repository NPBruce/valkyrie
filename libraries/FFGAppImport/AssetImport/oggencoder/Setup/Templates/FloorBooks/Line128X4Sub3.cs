namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X4Sub3 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 4, 3, 5, 3, 5, 3,
            5, 4, 6, 5, 6, 5, 7, 6, 6, 7, 7, 9, 9, 11, 11, 16,
            11, 14, 10, 11, 11, 13, 16, 15, 15, 15, 15, 15, 15, 15, 15, 15
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}