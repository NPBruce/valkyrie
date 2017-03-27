namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line512X17_3Sub2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 2, 3, 3, 4, 3, 5, 4, 6, 4, 6, 5, 7, 6, 7,
            6, 8, 6, 8, 7, 9, 8, 10, 8, 12, 9, 13, 10, 15, 10, 15,
            11, 14
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}