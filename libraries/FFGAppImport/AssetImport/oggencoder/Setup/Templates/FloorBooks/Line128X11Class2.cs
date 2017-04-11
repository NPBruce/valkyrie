namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X11Class2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            1, 6, 12, 16, 4, 12, 15, 16, 9, 15, 16, 16, 16, 16, 16, 16,
            2, 5, 11, 16, 5, 11, 13, 16, 9, 13, 16, 16, 16, 16, 16, 16,
            4, 8, 12, 16, 5, 9, 12, 16, 9, 13, 15, 16, 16, 16, 16, 16,
            15, 16, 16, 16, 11, 14, 13, 16, 12, 15, 16, 16, 16, 16, 16, 15
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}