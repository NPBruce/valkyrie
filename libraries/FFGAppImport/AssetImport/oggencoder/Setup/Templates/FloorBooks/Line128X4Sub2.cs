namespace OggVorbisEncoder.Setup.Templates.FloorBooks
{
    public class Line128X4Sub2 : IStaticCodeBook
    {
        new public int Dimensions = 1;

        new public byte[] LengthList = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 4, 3, 4, 3,
            4, 4, 5, 4, 5, 4, 6, 5, 6
        };

        new public CodeBookMapType MapType = CodeBookMapType.None;
        new public int QuantMin = 0;
        new public int QuantDelta = 0;
        new public int Quant = 0;
        new public int QuantSequenceP = 0;
        new public int[] QuantList = null;
    }
}