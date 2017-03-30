namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line1024X27_2Sub0 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            1, 5, 5, 5, 5, 5, 5, 5, 6, 5, 6, 5, 6, 5, 6, 5,
            6, 6, 7, 7, 7, 7, 8, 7, 8, 8, 9, 8, 10, 9, 10, 9
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}