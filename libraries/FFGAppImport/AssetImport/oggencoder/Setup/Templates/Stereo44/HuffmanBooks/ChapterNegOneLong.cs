namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class ChapterNegOneLong : IStaticCodeBook
    {
        new public int Dimensions = 2;

        new public byte[] LengthList = {
            4, 4, 7, 8, 7, 8, 10, 12, 17, 3, 1, 6, 6, 7, 8, 10,
            12, 15, 7, 6, 9, 9, 9, 11, 12, 14, 17, 8, 6, 9, 6, 7,
            9, 11, 13, 17, 7, 6, 9, 7, 7, 8, 9, 12, 15, 8, 8, 10,
            8, 7, 7, 7, 10, 14, 9, 10, 12, 10, 8, 8, 8, 10, 14, 11,
            13, 15, 13, 12, 11, 11, 12, 16, 17, 18, 18, 19, 20, 18, 16, 16,
            20
        };

        new public CodeBookMapType MapType = CodeBookMapType.None;
        new public int QuantMin = 0;
        new public int QuantDelta = 0;
        new public int Quant = 0;
        new public int QuantSequenceP = 0;
        new public int[] QuantList = null;
    }
}