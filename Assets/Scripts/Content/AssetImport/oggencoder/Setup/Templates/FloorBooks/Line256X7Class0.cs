namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line256X7Class0 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            7, 5, 5, 9, 9, 6, 6, 9, 12, 8, 7, 8, 11, 8, 9, 15,
            6, 3, 3, 7, 7, 4, 3, 6, 9, 6, 5, 6, 8, 6, 8, 15,
            8, 5, 5, 9, 8, 5, 4, 6, 10, 7, 5, 5, 11, 8, 7, 15,
            14, 15, 13, 13, 13, 13, 8, 11, 15, 10, 7, 6, 11, 9, 10, 15
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}