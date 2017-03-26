using System.Linq;

namespace OggVorbisEncoder.Setup
{
    public class Floor
    {
        public Floor(
            int[] partitionClass,
            int[] classDimensions,
            int[] classSubs,
            int[] classBook,
            int[][] classSubBook,
            int mult,
            int[] postList,
            float maxOver,
            float maxUnder,
            float maxError,
            float twoFitWeight,
            float twoFitAtten,
            int n)
        {
            PartitionClass = partitionClass;
            ClassDimensions = classDimensions;
            ClassSubs = classSubs;
            ClassBook = classBook;
            ClassSubBook = classSubBook;
            Mult = mult;
            PostList = postList;
            MaxOver = maxOver;
            MaxUnder = maxUnder;
            MaxError = maxError;
            TwoFitWeight = twoFitWeight;
            TwoFitAtten = twoFitAtten;
            N = n;
        }

        /// <summary>
        ///     0 to 15
        /// </summary>
        public int[] PartitionClass { get; } // VIF_PARTS length

        /// <summary>
        ///     1 to 8
        /// </summary>
        public int[] ClassDimensions { get; }

        /// <summary>
        ///     0,1,2,3 (bits: 1&lt;&lt;n poss)
        /// </summary>
        public int[] ClassSubs { get; }

        /// <summary>
        ///     subs ^ dim entries
        /// </summary>
        public int[] ClassBook { get; }

        /// <summary>
        ///     [VIF_CLASS][subs] [VIF_CLASS][8]
        /// </summary>
        public int[][] ClassSubBook { get; }

        /// <summary>
        ///     1 2 3 or 4
        /// </summary>
        public int Mult { get; }

        /// <summary>
        ///     first two implicit
        /// </summary>
        public int[] PostList { get; }

        /* encode side analysis parameters */
        public float MaxOver { get; }
        public float MaxUnder { get; }
        public float MaxError { get; }
        public float TwoFitWeight { get; }
        public float TwoFitAtten { get; }
        public int N { get; set; }

        public Floor Clone() =>
            new Floor(
                PartitionClass.ToArray(),
                ClassDimensions.ToArray(),
                ClassSubs.ToArray(),
                ClassBook.ToArray(),
                ClassSubBook.Select(s => s.ToArray()).ToArray(),
                Mult,
                PostList.ToArray(),
                MaxOver,
                MaxUnder,
                MaxError,
                TwoFitWeight,
                TwoFitAtten,
                N);
    }
}