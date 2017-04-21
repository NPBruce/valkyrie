namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X11_3Sub2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 5, 3, 5, 4, 6, 4, 6, 4, 7, 4, 7, 4, 8, 4,
            8, 4, 9, 4, 9, 4, 10, 4, 10, 5, 10, 5, 11, 5, 12, 6,
            12, 6
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}