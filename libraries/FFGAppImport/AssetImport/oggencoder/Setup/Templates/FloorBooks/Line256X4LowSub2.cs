namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line256X4LowSub2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 4, 3, 4,
            4, 4, 4, 4, 5, 5, 5, 6, 6
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}