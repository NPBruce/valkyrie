namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line1024X27_1Sub1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            8, 5, 8, 4, 9, 4, 9, 4, 9, 4, 9, 4, 9, 4, 9, 4,
            9, 4, 9, 4, 9, 4, 8, 4, 8, 4, 9, 5, 9, 5, 9, 5,
            9, 5, 9, 6, 10, 6, 10, 7, 10, 8, 11, 9, 11, 11, 12, 13,
            12, 14, 13, 15, 13, 15, 14, 16, 14, 17, 15, 17, 15, 15, 16, 16,
            15, 16, 16, 16, 15, 18, 16, 15, 17, 17, 19, 19, 19, 19, 19, 19,
            19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}