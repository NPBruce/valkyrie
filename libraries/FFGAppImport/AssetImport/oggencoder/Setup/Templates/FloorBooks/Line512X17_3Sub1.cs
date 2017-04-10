namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line512X17_3Sub1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 4, 4, 4, 4, 4, 4, 3, 4, 4, 4, 4, 4, 5, 4, 5,
            5, 5
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}