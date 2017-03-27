namespace OggVorbisEncoder.Setup
{
    public class Residue
    {
        public Residue(
            int begin,
            int end,
            int grouping,
            int partitions,
            int partitionValues,
            int groupBook,
            int[] secondStages,
            int[] bookList,
            int[] classMetric1,
            int[] classMetric2,
            ResidueType residueType)
        {
            Begin = begin;
            End = end;
            Grouping = grouping;
            Partitions = partitions;
            PartitionValues = partitionValues;
            GroupBook = groupBook;
            SecondStages = secondStages.ToFixedLength(64);
            BookList = bookList.ToFixedLength(512);
            ClassMetric1 = classMetric1.ToFixedLength(64);
            ClassMetric2 = classMetric2.ToFixedLength(64);
            ResidueType = residueType;
        }

        public int Begin;
        public int End { get; set; }
        public int Partitions;
        public int PartitionValues;
        public int GroupBook { get; set; }
        public int[] SecondStages;
        public int[] BookList;
        public int[] ClassMetric1;
        public int[] ClassMetric2;

        public ResidueType ResidueType;
        public int Grouping;

        public Residue Clone(ResidueType residueTypeOverride, int groupingOverride)
        {
            return new Residue(
                Begin,
                End,
                groupingOverride,
                Partitions,
                PartitionValues,
                GroupBook,
                SecondStages,
                BookList,
                ClassMetric1,
                ClassMetric2,
                residueTypeOverride);
        }
    }
}