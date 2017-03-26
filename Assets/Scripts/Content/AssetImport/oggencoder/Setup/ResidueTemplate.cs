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

        public ResidueType ResidueType { get; }
        public ResidueLimitType LimitType { get; }
        public int Grouping { get; }
        public Residue Residue { get; }
        public IStaticCodeBook BookAux { get; }
        public IStaticCodeBook BookAuxManaged { get; }
        public IStaticBookBlock BooksBase { get; }
        public IStaticBookBlock BooksBaseManaged { get; }
    }
}