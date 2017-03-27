namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line1024X27_3Sub2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 3, 3, 4, 3, 4, 4, 4, 4, 5, 5, 5, 5, 5, 6,
            5, 7, 5, 8, 6, 8, 6, 9, 7, 10, 7, 10, 8, 10, 8, 11,
            9, 11
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}