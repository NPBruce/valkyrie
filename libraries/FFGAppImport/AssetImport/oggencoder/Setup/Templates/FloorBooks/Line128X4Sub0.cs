namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X4Sub0 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            2, 2, 2, 2
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}