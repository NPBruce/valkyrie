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
        public int[] PartitionClass; // VIF_PARTS length

        /// <summary>
        ///     1 to 8
        /// </summary>
        public int[] ClassDimensions;

        /// <summary>
        ///     0,1,2,3 (bits: 1&lt;&lt;n poss)
        /// </summary>
        public int[] ClassSubs;

        /// <summary>
        ///     subs ^ dim entries
        /// </summary>
        public int[] ClassBook;

        /// <summary>
        ///     [VIF_CLASS][subs] [VIF_CLASS][8]
        /// </summary>
        public int[][] ClassSubBook;

        /// <summary>
        ///     1 2 3 or 4
        /// </summary>
        public int Mult;

        /// <summary>
        ///     first two implicit
        /// </summary>
        public int[] PostList;

        /* encode side analysis parameters */
        public float MaxOver;
        public float MaxUnder;
        public float MaxError;
        public float TwoFitWeight;
        public float TwoFitAtten;
        public int N { get; set; }

        public Floor Clone()
        {
            return new Floor(
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
}