using System;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{
    public static class StaticCodeBookExtensions
    {
        public static int GetQuantVals(this IStaticCodeBook book)
        {
            var vals = (int) Math.Floor(Math.Pow(book.LengthList.Length, 1f/book.Dimensions));

            // the above *should* be reliable, but we'll not assume that FP is
            // ever reliable when bitstream sync is at stake; verify via integer
            // means that vals really is the greatest value of dim for which
            // vals^b->bim <= b->entries 
            // treat the above as an initial guess 
            while (true)
            {
                var acc = 1;
                var acc1 = 1;

                for (var i = 0; i < book.Dimensions; i++)
                {
                    acc *= vals;
                    acc1 *= vals + 1;
                }

                if ((acc <= book.LengthList.Length) && (acc1 > book.LengthList.Length))
                    return vals;

                if (acc > book.LengthList.Length)
                    vals--;
                else
                    vals++;
            }
        }
    }
}