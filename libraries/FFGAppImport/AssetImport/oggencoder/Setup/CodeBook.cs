﻿using System;

namespace OggVorbisEncoder.Setup
{
    public class CodeBook
    {
        public CodeBook(
            int dimensions,
            int entries,
            int usedEntries,
            IStaticCodeBook staticBook,
            float[] valueList,
            uint[] codeList,
            int[] decIndex,
            byte[] decCodeLengths,
            uint[] decFirstTable,
            int decFirstTableN,
            int decMaxLength,
            int quantValues,
            int minVal,
            int delta)
        {
            Dimensions = dimensions;
            Entries = entries;
            UsedEntries = usedEntries;
            StaticBook = staticBook;
            ValueList = valueList;
            CodeList = codeList;
            DecIndex = decIndex;
            DecCodeLengths = decCodeLengths;
            DecFirstTable = decFirstTable;
            DecFirstTableN = decFirstTableN;
            DecMaxLength = decMaxLength;
            QuantValues = quantValues;
            MinVal = minVal;
            Delta = delta;
        }

        /// <summary>
        ///     codebook dimensions (elements per vector)
        /// </summary>
        public int Dimensions;

        /// <summary>
        ///     codebook entries
        /// </summary>
        public int Entries;

        /// <summary>
        ///     populated codebook entries
        /// </summary>
        public int UsedEntries;

        public IStaticCodeBook StaticBook;

        /* for encode, the below are entry-ordered, fully populated */
        /* for decode, the below are ordered by bitreversed codeword and only
           used entries are populated */

        /// <summary>
        ///     list of dim*entries actual entry values
        /// </summary>
        public float[] ValueList;

        /// <summary>
        ///     list of bitstream codewords for each entry
        /// </summary>
        public uint[] CodeList;

        /// <summary>
        ///     only used if sparseness collapsed
        /// </summary>
        public int[] DecIndex;

        public byte[] DecCodeLengths;


        public uint[] DecFirstTable;

        public int DecFirstTableN;

        public int DecMaxLength;

        /* The current encoder uses only centered, integer-only lattice books. */
        public int QuantValues;
        public int MinVal;
        public int Delta;

        public static CodeBook InitEncode(IStaticCodeBook source)
        {
            return new CodeBook(
                source.Dimensions,
                source.LengthList.Length,
                source.LengthList.Length,
                source,
                null,
                Encoding.MakeWords(source.LengthList, 0),
                null,
                null,
                null,
                0,
                0,
                source.GetQuantVals(),
                (int) Math.Round(Encoding.UnpackFloat(source.QuantMin)),
                (int) Math.Round(Encoding.UnpackFloat(source.QuantDelta))
            );
        }
    }
}