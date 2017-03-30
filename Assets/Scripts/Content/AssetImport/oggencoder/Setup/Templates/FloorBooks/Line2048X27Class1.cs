namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line2048X27Class1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            2, 6, 8, 9, 7, 11, 13, 13, 1, 3, 5, 5, 6, 6, 12, 10
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}