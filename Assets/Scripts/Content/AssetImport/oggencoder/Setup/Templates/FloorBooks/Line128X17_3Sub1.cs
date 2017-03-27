namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X17_3Sub1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 4, 4, 4, 4, 4, 4, 4, 5, 3, 5, 3, 5, 4, 6, 4,
            6, 4
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}