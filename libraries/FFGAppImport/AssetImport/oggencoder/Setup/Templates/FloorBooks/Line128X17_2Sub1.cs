namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X17_2Sub1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 4, 5, 4, 6, 4, 8, 3, 9, 3, 9, 2, 9, 3, 8, 4,
            9, 4
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}