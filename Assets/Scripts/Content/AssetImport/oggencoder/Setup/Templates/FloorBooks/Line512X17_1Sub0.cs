namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line512X17_1Sub0 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            2, 4, 5, 4, 5, 4, 5, 4, 5, 5, 5, 5, 5, 5, 6, 5,
            6, 5, 6, 6, 7, 6, 7, 6, 8, 7, 8, 7, 8, 7, 8, 7
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}