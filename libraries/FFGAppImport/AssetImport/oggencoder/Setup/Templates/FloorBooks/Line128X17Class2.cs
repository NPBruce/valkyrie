namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X17Class2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            1, 4, 10, 19, 3, 8, 13, 19, 7, 12, 19, 19, 19, 19, 19, 19,
            2, 6, 11, 19, 8, 13, 19, 19, 9, 11, 19, 19, 19, 19, 19, 19,
            6, 7, 13, 19, 9, 13, 19, 19, 10, 13, 18, 18, 18, 18, 18, 18,
            18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}