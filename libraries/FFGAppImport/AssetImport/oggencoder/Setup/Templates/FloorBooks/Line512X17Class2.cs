namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line512X17Class2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            3, 3, 3, 14, 5, 4, 4, 11, 8, 6, 6, 10, 17, 12, 11, 17,
            6, 5, 5, 15, 5, 3, 4, 11, 8, 5, 5, 8, 16, 9, 10, 14,
            10, 8, 9, 17, 8, 6, 6, 13, 10, 7, 7, 10, 16, 11, 13, 14,
            17, 17, 17, 17, 17, 16, 16, 16, 16, 15, 16, 16, 16, 16, 16, 16
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}