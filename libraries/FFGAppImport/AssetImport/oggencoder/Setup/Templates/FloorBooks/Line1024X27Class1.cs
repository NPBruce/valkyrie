namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line1024X27Class1 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            2, 10, 8, 14, 7, 12, 11, 14, 1, 5, 3, 7, 4, 9, 7, 13
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}