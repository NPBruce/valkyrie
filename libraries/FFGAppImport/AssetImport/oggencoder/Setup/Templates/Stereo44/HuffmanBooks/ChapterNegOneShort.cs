namespace OggVorbisEncoder.Setup.Templates.Stereo44.HuffmanBooks
{
    public class ChapterNegOneShort : IStaticCodeBook
    {
        new public int Dimensions = 2;

        new public byte[] LengthList = {
            10, 9, 12, 15, 12, 13, 16, 14, 16, 7, 1, 5, 14, 7, 10, 13,
            16, 16, 9, 4, 6, 16, 8, 11, 16, 16, 16, 14, 4, 7, 16, 9,
            12, 14, 16, 16, 10, 5, 7, 14, 9, 12, 14, 15, 15, 13, 8, 9,
            14, 10, 12, 13, 14, 15, 13, 9, 9, 7, 6, 8, 11, 12, 12, 14,
            8, 8, 5, 4, 5, 8, 11, 12, 16, 10, 10, 6, 5, 6, 8, 9,
            10
        };

        new public CodeBookMapType MapType = CodeBookMapType.None;
        new public int QuantMin = 0;
        new public int QuantDelta = 0;
        new public int Quant = 0;
        new public int QuantSequenceP = 0;
        new public int[] QuantList = null;
    }
}