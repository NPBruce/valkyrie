using System;
using System.Collections.Generic;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{
    public class ResidueLookup
    {
        private readonly CodeBook[][] _partitionBooks;
        private readonly CodeBook _phraseBook;
        private readonly Residue _residue;
        private readonly int _stages;

        public ResidueLookup(Residue residue, List<CodeBook> fullBooks)
        {
            if (residue.ResidueType != ResidueType.Two)
                throw new NotImplementedException("ResidueTypes other than 'Two' are not yet implemented");

            _residue = residue;

            _phraseBook = fullBooks[residue.GroupBook];

            var acc = 0;
            var maxstage = 0;

            _partitionBooks = new CodeBook[residue.Partitions][];

            for (var j = 0; j < _partitionBooks.Length; j++)
            {
                var stages = Encoding.Log(residue.SecondStages[j]);
                if (stages == 0)
                    continue;

                if (stages > maxstage)
                    maxstage = stages;

                _partitionBooks[j] = new CodeBook[stages];

                for (var k = 0; k < stages; k++)
                    if ((residue.SecondStages[j] & (1 << k)) != 0)
                        _partitionBooks[j][k] = fullBooks[residue.BookList[acc++]];
            }

            _stages = maxstage;
        }

        public int Forward(
            EncodeBuffer buffer,
            int pcmend,
            int[][] couples,
            bool[] nonzero,
            int channels,
            int[][] partword)
        {
            return Res2Forward(buffer, pcmend, couples, nonzero, channels, partword);
        }

        private int Res2Forward(EncodeBuffer buffer, int pcmend, int[][] couples, bool[] nonzero, int channels,
            int[][] partword)
        {
            var n = pcmend/2;

            var used = false;

            // don't duplicate the code; use a working vector hack for now and
            // reshape ourselves into a single channel res1
            var work = new int[channels*n];
            for (var i = 0; i < channels; i++)
            {
                var pcm = couples[i];
                used = used || nonzero[i];

                for (int j = 0, k = i; j < n; j++, k += channels)
                    work[k] = pcm[j];
            }

            return used
                ? Res1Forward(buffer, work, 1, partword)
                : 0;
        }

        private int Res1Forward(EncodeBuffer buffer, int[] work, int channels, int[][] partword)
        {
            var n = _residue.End - _residue.Begin;
            var partitionValues = n/_residue.Grouping;

            // we code the partition words for each channel, then the residual
            // words for a partition per channel until we've written all the
            // residual words for that partition word.  Then write the next
            // partition channel words
            for (var s = 0; s < _stages; s++)
                for (var i = 0; i < partitionValues;)
                {
                    // first we encode a partition codeword for each channel 
                    if (s == 0)
                        for (var j = 0; j < channels; j++)
                        {
                            var val = partword[j][i];
                            for (var k = 1; k < _phraseBook.Dimensions; k++)
                            {
                                val *= _residue.Partitions;
                                if (i + k < partitionValues)
                                    val += partword[j][i + k];
                            }

                            if (val < _phraseBook.Entries)
                                buffer.WriteBook(_phraseBook, val);
                        }

                    // now we encode interleaved residual values for the partitions 
                    for (var k = 0; (k < _phraseBook.Dimensions) && (i < partitionValues); k++,i++)
                    {
                        var offset = i*_residue.Grouping + _residue.Begin;

                        for (var j = 0; j < channels; j++)
                            if ((_residue.SecondStages[partword[j][i]] & (1 << s)) != 0)
                            {
                                var statebook = _partitionBooks[partword[j][i]][s];
                                if (statebook != null)
                                    EncodePart(buffer, work, j + offset, _residue.Grouping, statebook);
                            }
                    }
                }

            return 0;
        }

        private void EncodePart(EncodeBuffer buffer, int[] vec, int offset, int n, CodeBook book)
        {
            var step = n/book.Dimensions;

            for (var i = 0; i < step; i++)
            {
                var entry = LocalBookBestError(book, vec, offset + i*book.Dimensions);
                buffer.WriteBook(book, entry);
            }
        }

        private static int LocalBookBestError(CodeBook book, int[] vec, int offset)
        {
            int i;
            int o;
            var ze = book.QuantValues >> 1;
            var index = 0;

            // assumes integer/centered encoder codebook maptype 1 no more than dim 8 
            var p = new int[8];
            if (book.Delta != 1)
                for (i = 0, o = book.Dimensions; i < book.Dimensions; i++)
                {
                    var v = (vec[offset + --o] - book.MinVal + (book.Delta >> 1))/book.Delta;
                    var m = v < ze ? ((ze - v) << 1) - 1 : (v - ze) << 1;
                    index = index*book.QuantValues + (m < 0 ? 0 : (m >= book.QuantValues ? book.QuantValues - 1 : m));
                    p[o] = v*book.Delta + book.MinVal;
                }
            else
                for (i = 0, o = book.Dimensions; i < book.Dimensions; i++)
                {
                    var v = vec[offset + --o] - book.MinVal;
                    var m = v < ze ? ((ze - v) << 1) - 1 : (v - ze) << 1;
                    index = index*book.QuantValues + (m < 0 ? 0 : (m >= book.QuantValues ? book.QuantValues - 1 : m));
                    p[o] = v*book.Delta + book.MinVal;
                }

            if (book.StaticBook.LengthList[index] <= 0)
            {
                // assumes integer/centered encoder codebook maptype 1 no more than dim 8 
                var best = -1;
                var e = new int[8];
                var maxval = book.MinVal + book.Delta*(book.QuantValues - 1);
                for (i = 0; i < book.Entries; i++)
                {
                    if (book.StaticBook.LengthList[i] > 0)
                    {
                        var current = 0;
                        for (var j = 0; j < book.Dimensions; j++)
                        {
                            var val = e[j] - vec[offset + j];
                            current += val*val;
                        }

                        if ((best == -1) || (current < best))
                        {
                            for (var x = 0; x < e.Length; x++)
                                p[x] = e[x];

                            best = current;
                            index = i;
                        }
                    }

                    // assumes the value patterning created by the tools in vq
                    var l = 0;
                    while (e[l] >= maxval)
                        e[l++] = 0;

                    if (e[l] >= 0)
                        e[l] += book.Delta;

                    e[l] = -e[l];
                }
            }

            if (index > -1)
                for (i = 0; i < book.Dimensions; i++)
                    vec[offset++] -= p[i];

            return index;
        }

        public int[][] Class(int[][] couples, IList<bool> nonzero, int channels)
        {
            for (var channel = 0; channel < channels; channel++)
                if (nonzero[channel])
                    return ResTwoClass(new List<int[]>(couples), channels);

            return null;
        }

        private int[][] ResTwoClass(List<int[]> couples, int channels)
        {
            var n = _residue.End - _residue.Begin;

            var valueCount = n/_residue.Grouping;

            var partword = new int[1][];
            partword[0] = new int[valueCount];

            for (int i = 0, l = _residue.Begin/channels; i < valueCount; i++)
            {
                var magMax = 0;
                var angMax = 0;
                for (var g = 0; g < _residue.Grouping; g += channels)
                {
                    var abs = Math.Abs(couples[0][l]);
                    if (abs > magMax)
                        magMax = abs;

                    for (var k = 1; k < channels; k++)
                    {
                        abs = Math.Abs(couples[k][l]);
                        if (abs > angMax)
                            angMax = abs;
                    }

                    l++;
                }

                int j;
                for (j = 0; j < _residue.Partitions - 1; j++)
                    if ((magMax <= _residue.ClassMetric1[j])
                        && (angMax <= _residue.ClassMetric2[j]))
                        break;

                partword[0][i] = j;
            }

            return partword;
        }
    }
}