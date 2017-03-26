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

        public int Begin { get; }
        public int End { get; set; }
        public int Partitions { get; }
        public int PartitionValues { get; }
        public int GroupBook { get; set; }
        public int[] SecondStages { get; }
        public int[] BookList { get; }
        public int[] ClassMetric1 { get; }
        public int[] ClassMetric2 { get; }

        public ResidueType ResidueType { get; }
        public int Grouping { get; }

        public Residue Clone(ResidueType residueTypeOverride, int groupingOverride)
            => new Residue(
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