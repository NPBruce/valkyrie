namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter6
{
    public class Page9_1 : IStaticCodeBook
    {
        public int Dimensions = 2;

        public byte[] LengthList = {
            1, 4, 4, 7, 7, 7, 7, 7, 6, 8, 8, 8, 8, 6, 6, 6,
            8, 8, 8, 8, 8, 7, 9, 8, 10, 10, 5, 6, 6, 8, 8, 9,
            9, 8, 8, 10, 10, 10, 10, 16, 9, 9, 9, 9, 9, 9, 9, 8,
            10, 9, 11, 11, 16, 8, 9, 9, 9, 9, 9, 9, 9, 10, 10, 11,
            11, 16, 13, 13, 9, 9, 10, 9, 9, 10, 11, 11, 11, 12, 16, 13,
            14, 9, 8, 10, 8, 9, 9, 10, 10, 12, 11, 16, 14, 16, 9, 9,
            9, 9, 11, 11, 12, 11, 12, 11, 16, 16, 16, 9, 7, 9, 6, 11,
            11, 11, 10, 11, 11, 16, 16, 16, 11, 12, 9, 10, 11, 11, 12, 11,
            13, 13, 16, 16, 16, 12, 11, 10, 7, 12, 10, 12, 12, 12, 12, 16,
            16, 15, 16, 16, 10, 11, 10, 11, 13, 13, 14, 12, 16, 16, 16, 15,
            15, 12, 10, 11, 11, 13, 11, 12, 13
        };

        public CodeBookMapType MapType = CodeBookMapType.Implicit;
        public int QuantMin = -518889472;
        public int QuantDelta = 1622704128;
        public int Quant = 4;
        public int QuantSequenceP = 0;

        public int[] QuantList = {
            6,
            5,
            7,
            4,
            8,
            3,
            9,
            2,
            10,
            1,
            11,
            0,
            12
        };
    }
}