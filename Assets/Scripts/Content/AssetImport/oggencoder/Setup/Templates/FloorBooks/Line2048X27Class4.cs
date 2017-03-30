namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line2048X27Class4 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            2, 4, 7, 13, 4, 5, 7, 15, 8, 7, 10, 16, 16, 14, 16, 16,
            2, 4, 7, 16, 3, 4, 7, 14, 8, 8, 10, 16, 16, 16, 15, 16,
            6, 8, 11, 16, 7, 7, 9, 16, 11, 9, 13, 16, 16, 16, 15, 16,
            16, 16, 16, 16, 14, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}