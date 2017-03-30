namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X17Class1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            1, 3, 4, 7, 2, 5, 6, 7
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}