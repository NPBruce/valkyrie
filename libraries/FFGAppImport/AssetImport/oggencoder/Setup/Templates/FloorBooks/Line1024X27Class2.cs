namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line1024X27Class2 : IStaticCodeBook
    {
        public int Dimensions = 1;

        public byte[] LengthList = {
            1, 4, 2, 6, 3, 7, 5, 7
        };

        public CodeBookMapType MapType = CodeBookMapType.None;
        public int QuantMin = 0;
        public int QuantDelta = 0;
        public int Quant  = 0;
        public int QuantSequenceP = 0;
        public int[] QuantList = null;
    }
}