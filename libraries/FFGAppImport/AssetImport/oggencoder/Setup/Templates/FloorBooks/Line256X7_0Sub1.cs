namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line256X7_0Sub1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 2, 3, 3, 3, 3, 4, 3, 4
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}