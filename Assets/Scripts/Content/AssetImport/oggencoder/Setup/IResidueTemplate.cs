namespace OggVorbisEncoder.Setup
{
    public class IResidueTemplate
    {
        ResidueType ResidueType;
        ResidueLimitType LimitType;
        int Grouping;
        Residue Residue;
        IStaticCodeBook BookAux;
        IStaticCodeBook BookAuxManaged;
        IStaticBookBlock BooksBase;
        IStaticBookBlock BooksBaseManaged;
    }
}