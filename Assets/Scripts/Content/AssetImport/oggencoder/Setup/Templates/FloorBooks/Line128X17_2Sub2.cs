namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X17_2Sub2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 5, 1, 5, 3, 5, 3, 5, 4, 7, 5, 10, 7, 10, 7,
            12, 10, 14, 10, 14, 9, 14, 11, 14, 14, 14, 13, 13, 13, 13, 13,
            13, 13
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}