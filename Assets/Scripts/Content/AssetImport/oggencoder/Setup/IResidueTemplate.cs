namespace OggVorbisEncoder.Setup
{
    public class IResidueTemplate
    {
        public ResidueType ResidueType;
        public ResidueLimitType LimitType;
        public int Grouping;
        public Residue Residue;
        public IStaticCodeBook BookAux;
        public IStaticCodeBook BookAuxManaged;
        public IStaticBookBlock BooksBase;
        public IStaticBookBlock BooksBaseManaged;
    }
}