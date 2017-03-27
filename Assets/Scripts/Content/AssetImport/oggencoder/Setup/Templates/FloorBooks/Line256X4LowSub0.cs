namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line256X4LowSub0 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            1, 3, 2, 3
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}