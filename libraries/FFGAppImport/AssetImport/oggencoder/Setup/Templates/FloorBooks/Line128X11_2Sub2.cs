namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X11_2Sub2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 3, 3, 3, 4, 4, 4, 4, 5, 4, 5, 4, 6, 5, 7,
            5, 7, 6, 8, 6, 8, 6, 9, 7, 9, 7, 10, 7, 9, 8, 11,
            8, 11
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}