namespace OggVorbisEncoder.Setup
{
    public class ResidueTemplate : IResidueTemplate
    {
        public ResidueTemplate(
            ResidueType residueType,
            ResidueLimitType limitType,
            int grouping,
            Residue residue,
            IStaticCodeBook bookAux,
            IStaticCodeBook booxAuxManaged,
            IStaticBookBlock booksBase,
            IStaticBookBlock booksBaseManaged)
        {
            ResidueType = residueType;
            LimitType = limitType;
            Residue = residue;
            BookAux = bookAux;
            BookAuxManaged = booxAuxManaged;
            BooksBase = booksBase;
            BooksBaseManaged = booksBaseManaged;
            Grouping = grouping;
        }

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