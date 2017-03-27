namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line1024X27_2Sub1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            4, 3, 4, 3, 4, 4, 5, 4, 5, 4, 5, 5, 6, 5, 6, 5,
            7, 5, 7, 6, 7, 6, 8, 7, 8, 7, 8, 7, 9, 8, 9, 9,
            9, 9, 10, 10, 10, 11, 9, 12, 9, 12, 9, 15, 10, 14, 9, 13,
            10, 13, 10, 12, 10, 12, 10, 13, 10, 12, 11, 13, 11, 14, 12, 13,
            13, 14, 14, 13, 14, 15, 14, 16, 13, 13, 14, 16, 16, 16, 16, 16,
            16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 15, 15
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}