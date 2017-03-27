namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X7_1Sub2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 4, 3, 6, 3, 6, 3,
            6, 3, 7, 3, 8, 4, 9, 4, 9
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}