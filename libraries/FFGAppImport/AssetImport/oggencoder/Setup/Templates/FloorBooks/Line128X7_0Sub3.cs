namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X7_0Sub3 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 5, 3, 5, 3, 5, 4,
            5, 4, 5, 5, 5, 5, 6, 5, 6, 5, 6, 5, 6, 5, 6, 5,
            7, 8, 9, 11, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}