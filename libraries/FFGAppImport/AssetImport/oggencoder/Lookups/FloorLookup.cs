using System;
using System.Collections.Generic;
using System.Linq;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{
    public class FloorLookup
    {
        private const int Posit = 63;
        private readonly Floor _floor;
        private readonly int[] _forwardIndex = new int[Posit + 2];
        private readonly int[] _highNeighbor = new int[Posit];

        private readonly int[] _lowNeighbor = new int[Posit];
        private readonly int _n;
        private readonly int _posts;
        private readonly int _quantQ;
        private readonly int[] _reverseIndex = new int[Posit + 2];
        private readonly int[] _sortedIndex = new int[Posit + 2];

        public FloorLookup(Floor floor)
        {
            _floor = floor;
            _n = floor.PostList[1];

            // we drop each position value in-between already decoded values,
            // and use linear interpolation to predict each new value past the
            // edges.  The positions are read in the order of the position
            // list... we precompute the bounding positions in the lookup.  Of
            // course, the neighbors can change (if a position is declined), but
            // this is an initial mapping 
            var n = 0;
            foreach (var partition in floor.PartitionClass)
                n += floor.ClassDimensions[partition];

            n += 2;
            _posts = n;

            // also store a sorted position index 
            var sorted = floor
                .PostList
                .Select((v, i) => new {Value = v, Index = i})
                .OrderBy(o => o.Value)
                .ToArray();

            // points from sort order back to range number 
            for (var i = 0; i < n; i++)
                _forwardIndex[i] = sorted[i].Index;

            // points from range order to sorted position 
            for (var i = 0; i < n; i++)
                _reverseIndex[_forwardIndex[i]] = i;

            // we actually need the post values too 
            for (var i = 0; i < n; i++)
                _sortedIndex[i] = floor.PostList[_forwardIndex[i]];

            // quantize values to multiplier spec 
            switch (floor.Mult)
            {
                case 1: // 1024 . 256 
                    _quantQ = 256;
                    break;
                case 2: // 1024 . 128 
                    _quantQ = 128;
                    break;
                case 3: // 1024 . 86 
                    _quantQ = 86;
                    break;
                case 4: // 1024 . 64 
                    _quantQ = 64;
                    break;
            }

            // discover our neighbors for decode where we don't use fit flags
            // (that would push the neighbors outward) 
            for (var i = 0; i < n - 2; i++)
            {
                var lo = 0;
                var hi = 1;
                var lx = 0;
                var hx = _n;
                var currentX = floor.PostList[i + 2];
                for (var j = 0; j < i + 2; j++)
                {
                    var x = floor.PostList[j];
                    if ((x > lx) && (x < currentX))
                    {
                        lo = j;
                        lx = x;
                    }
                    if ((x < hx) && (x > currentX))
                    {
                        hi = j;
                        hx = x;
                    }
                }
                _lowNeighbor[i] = lo;
                _highNeighbor[i] = hi;
            }
        }

        public int[] Fit(IList<float> logmdct, IList<float> logmask)
        {
            var n = _n;

            var nonzero = 0;
            var fits = new FitAccumulation[Posit + 1];
            var fitValueA = new int[Posit + 2]; // index by range list position 
            var fitValueB = new int[Posit + 2]; // index by range list position 

            var loneighbor = new int[Posit + 2]; // sorted index of range list position (+2) 
            var hineighbor = new int[Posit + 2];
            var memo = new int[Posit + 2];

            for (var i = 0; i < _posts; i++)
                fitValueA[i] = -200; // mark all unused 

            for (var i = 0; i < _posts; i++)
                fitValueB[i] = -200; // mark all unused 

            for (var i = 0; i < _posts; i++)
                loneighbor[i] = 0; // 0 for the implicit 0 post 

            for (var i = 0; i < _posts; i++)
                hineighbor[i] = 1; // 1 for the implicit post at n 

            for (var i = 0; i < _posts; i++)
                memo[i] = -1; // no neighbor yet 

            // quantize the relevant floor points and collect them into line fit
            // structures (one per minimal division) at the same time 
            if (_posts == 0)
                nonzero = AccumulateFit(logmask, logmdct, 0, n, ref fits[0], n);
            else
                for (var i = 0; i < _posts - 1; i++)
                    nonzero += AccumulateFit(logmask, logmdct, _sortedIndex[i], _sortedIndex[i + 1], ref fits[i], n);

            if (nonzero != 0)
            {
                // start by fitting the implicit base case.... 
                int y0, y1;
                FitLine(fits, 0, _posts - 1, out y0, out y1);

                fitValueA[0] = y0;
                fitValueB[0] = y0;
                fitValueB[1] = y1;
                fitValueA[1] = y1;

                // Non degenerate case 
                // start progressive splitting.  This is a greedy, non-optimal
                // algorithm, but simple and close enough to the best
                // answer. 
                for (var i = 2; i < _posts; i++)
                {
                    var sortPosition = _reverseIndex[i];
                    var ln = loneighbor[sortPosition];
                    var hn = hineighbor[sortPosition];

                    // eliminate repeat searches of a particular range with a memo 
                    if (memo[ln] != hn)
                    {
                        // haven't performed this error search yet 
                        var lowSortPosition = _reverseIndex[ln];
                        var highSortPosition = _reverseIndex[hn];
                        memo[ln] = hn;

                        {
                            // A note: we want to bound/minimize *local*, not global, error 
                            var lx = _floor.PostList[ln];
                            var hx = _floor.PostList[hn];
                            var ly = PostY(new List<int>(fitValueA), new List<int>(fitValueB), ln);
                            var hy = PostY(new List<int>(fitValueA), new List<int>(fitValueB), hn);

                            if ((ly == -1) || (hy == -1))
                                throw new InvalidOperationException("An error occurred during minimization");

                            if (InspectError(lx, hx, ly, hy, logmask, logmdct))
                            {
                                // outside error bounds/begin search area.  Split it. 
                                int ly0, ly1, hy0, hy1;
                                var ret0 = FitLine(fits, lowSortPosition, sortPosition - lowSortPosition, out ly0,
                                    out ly1);
                                var ret1 = FitLine(fits, sortPosition, highSortPosition - sortPosition, out hy0, out hy1);

                                if (ret0 != 0)
                                {
                                    ly0 = ly;
                                    ly1 = hy0;
                                }
                                if (ret1 != 0)
                                {
                                    hy0 = ly1;
                                    hy1 = hy;
                                }

                                if ((ret0 != 0) && (ret1 != 0))
                                {
                                    fitValueA[i] = -200;
                                    fitValueB[i] = -200;
                                }
                                else
                                {
                                    // store new edge values 
                                    fitValueB[ln] = ly0;
                                    if (ln == 0) fitValueA[ln] = ly0;
                                    fitValueA[i] = ly1;
                                    fitValueB[i] = hy0;
                                    fitValueA[hn] = hy1;
                                    if (hn == 1) fitValueB[hn] = hy1;

                                    if ((ly1 >= 0) || (hy0 >= 0))
                                    {
                                        // store new neighbor values 
                                        for (var j = sortPosition - 1; j >= 0; j--)
                                            if (hineighbor[j] == hn)
                                                hineighbor[j] = i;
                                            else
                                                break;

                                        for (var j = sortPosition + 1; j < _posts; j++)
                                            if (loneighbor[j] == ln)
                                                loneighbor[j] = i;
                                            else
                                                break;
                                    }
                                }
                            }
                            else
                            {
                                fitValueA[i] = -200;
                                fitValueB[i] = -200;
                            }
                        }
                    }
                }

                var output = new int[_posts];

                output[0] = PostY(new List<int>(fitValueA), new List<int>(fitValueB), 0);
                output[1] = PostY(new List<int>(fitValueA), new List<int>(fitValueB), 1);

                // fill in posts marked as not using a fit; we will zero
                // back out to 'unused' when encoding them so int as curve
                // interpolation doesn't force them into use 
                for (var i = 2; i < _posts; i++)
                {
                    var ln = _lowNeighbor[i - 2];
                    var hn = _highNeighbor[i - 2];
                    var x0 = _floor.PostList[ln];
                    var x1 = _floor.PostList[hn];
                    y0 = output[ln];
                    y1 = output[hn];

                    var predicted = RenderPoint(x0, x1, y0, y1, _floor.PostList[i]);
                    var vx = PostY(new List<int>(fitValueA), new List<int>(fitValueB), i);

                    if ((vx >= 0) && (predicted != vx))
                        output[i] = vx;
                    else
                        output[i] = predicted | 0x8000;
                }

                return output;
            }

            return null;
        }

        private bool InspectError(int x0, int x1, int y0, int y1, IList<float> mask, IList<float> mdct)
        {
            var dy = y1 - y0;
            var adx = x1 - x0;
            var ady = Math.Abs(dy);
            var baseVal = dy/adx;
            var sy = dy < 0 ? baseVal - 1 : baseVal + 1;
            var x = x0;
            var y = y0;
            var err = 0;
            var val = DecibelQuant(mask[x]);
            var n = 0;

            ady -= Math.Abs(baseVal*adx);

            var mse = y - val;
            mse *= mse;
            n++;
            if (mdct[x] + _floor.TwoFitAtten >= mask[x])
            {
                if (y + _floor.MaxOver < val)
                    return true;

                if (y - _floor.MaxUnder > val)
                    return true;
            }

            while (++x < x1)
            {
                err = err + ady;
                if (err >= adx)
                {
                    err -= adx;
                    y += sy;
                }
                else
                {
                    y += baseVal;
                }

                val = DecibelQuant(mask[x]);
                mse += (y - val)*(y - val);
                n++;

                if (mdct[x] + _floor.TwoFitAtten >= mask[x])
                    if (val != 0)
                    {
                        if (y + _floor.MaxOver < val)
                            return true;

                        if (y - _floor.MaxUnder > val)
                            return true;
                    }
            }

            if (_floor.MaxOver*_floor.MaxOver/n > _floor.MaxError)
                return false;

            if (_floor.MaxUnder*_floor.MaxUnder/n > _floor.MaxError)
                return false;

            if (mse/n > _floor.MaxError)
                return true;

            return false;
        }

        private int FitLine(IList<FitAccumulation> acc, int offset, int fits, out int y0, out int y1)
        {
            y0 = -200;
            y1 = -200;

            double xb = 0, yb = 0, x2b = 0, xyb = 0, bn = 0;

            var x0 = acc[offset + 0].x0;
            var x1 = acc[offset + fits - 1].x1;

            for (var i = 0; i < fits; i++)
            {
                var weight = (acc[offset + i].bn + acc[offset + i].an)*_floor.TwoFitWeight/(acc[offset + i].an + 1) +
                             1.0;

                xb += acc[offset + i].xb + acc[offset + i].xa*weight;
                yb += acc[offset + i].yb + acc[offset + i].ya*weight;
                x2b += acc[offset + i].x2b + acc[offset + i].x2a*weight;
                xyb += acc[offset + i].xyb + acc[offset + i].xya*weight;
                bn += acc[offset + i].bn + acc[offset + i].an*weight;
            }

            if (y0 >= 0)
            {
                xb += x0;
                yb += y0;
                x2b += x0*x0;
                xyb += y0*x0;
                bn++;
            }

            if (y1 >= 0)
            {
                xb += x1;
                yb += y1;
                x2b += x1*x1;
                xyb += y1*x1;
                bn++;
            }

            {
                var denom = bn*x2b - xb*xb;

                if (denom > 0)
                {
                    var a = (yb*x2b - xyb*xb)/denom;
                    var b = (bn*xyb - xb*yb)/denom;
                    y0 = (int) Math.Round(a + b*x0);
                    y1 = (int) Math.Round(a + b*x1);

                    // limit to our range! 
                    if (y0 > 1023) y0 = 1023;
                    if (y1 > 1023) y1 = 1023;
                    if (y0 < 0) y0 = 0;
                    if (y1 < 0) y1 = 0;

                    return 0;
                }
                y0 = 0;
                y1 = 0;
                return 1;
            }
        }

        private static int RenderPoint(int x0, int x1, int y0, int y1, int x)
        {
            y0 &= 0x7fff; // mask off flag
            y1 &= 0x7fff;

            var dy = y1 - y0;
            var adx = x1 - x0;
            var ady = Math.Abs(dy);
            var err = ady*(x - x0);
            var off = err/adx;

            if (dy < 0)
                return y0 - off;

            return y0 + off;
        }

        private static int PostY(List<int> a, List<int> b, int pos)
        {
            if (a[pos] < 0)
                return b[pos];

            if (b[pos] < 0)
                return a[pos];

            return (a[pos] + b[pos]) >> 1;
        }

        private int AccumulateFit(IList<float> flr, IList<float> mdct, int x0, int x1, ref FitAccumulation fits, int n)
        {
            int xa = 0, ya = 0, x2a = 0, xya = 0, na = 0, xb = 0, yb = 0, x2b = 0, xyb = 0, nb = 0;

            fits.x0 = x0;
            fits.x1 = x1;

            if (x1 >= n)
                x1 = n - 1;

            for (var i = x0; i <= x1; i++)
            {
                var quantized = DecibelQuant(flr[i]);
                if (quantized != 0)
                    if (mdct[i] + _floor.TwoFitAtten >= flr[i])
                    {
                        xa += i;
                        ya += quantized;
                        x2a += i*i;
                        xya += i*quantized;
                        na++;
                    }
                    else
                    {
                        xb += i;
                        yb += quantized;
                        x2b += i*i;
                        xyb += i*quantized;
                        nb++;
                    }
            }

            fits.xa = xa;
            fits.ya = ya;
            fits.x2a = x2a;
            fits.xya = xya;
            fits.an = na;

            fits.xb = xb;
            fits.yb = yb;
            fits.x2b = x2b;
            fits.xyb = xyb;
            fits.bn = nb;

            return na;
        }

        private static int DecibelQuant(float x)
        {
            var i = (int) (x*7.3142857f + 1023.5f);

            if (i > 1023)
                return 1023;

            if (i < 0)
                return 0;

            return i;
        }

        public bool Encode(
            EncodeBuffer buffer,
            IList<IStaticCodeBook> staticBooks,
            IList<CodeBook> books,
            int[] post,
            int[] ilogmask,
            int pcmEnd,
            int n)
        {
            var output = new int[Posit + 2];

            // quantize values to multiplier spec 
            if (post != null)
            {
                for (var i = 0; i < _posts; i++)
                {
                    var val = post[i] & 0x7fff;
                    switch (_floor.Mult)
                    {
                        case 1: // 1024 . 256 
                            val >>= 2;
                            break;
                        case 2: // 1024 . 128 
                            val >>= 3;
                            break;
                        case 3: // 1024 . 86 
                            val /= 12;
                            break;
                        case 4: // 1024 . 64 
                            val >>= 4;
                            break;
                    }
                    post[i] = val | (post[i] & 0x8000);
                }

                output[0] = post[0];
                output[1] = post[1];

                // find prediction values for each post and subtract them 
                for (var i = 2; i < _posts; i++)
                {
                    var ln = _lowNeighbor[i - 2];
                    var hn = _highNeighbor[i - 2];
                    var x0 = _floor.PostList[ln];
                    var x1 = _floor.PostList[hn];
                    var y0 = post[ln];
                    var y1 = post[hn];

                    var predicted = RenderPoint(x0, x1, y0, y1, _floor.PostList[i]);

                    if (((post[i] & 0x8000) != 0) || (predicted == post[i]))
                    {
                        post[i] = predicted | 0x8000; // in case there was roundoff jitter in interpolation 
                        output[i] = 0;
                    }
                    else
                    {
                        var headroom = _quantQ - predicted < predicted
                            ? _quantQ - predicted
                            : predicted;

                        var val = post[i] - predicted;

                        // at this point the 'deviation' value is in the range +/- max
                        // range, but the real, unique range can always be mapped to
                        // only [0-maxrange).  So we want to wrap the deviation into
                        // this limited range, but do it in the way that least screws
                        // an essentially gaussian probability distribution. 

                        if (val < 0)
                            if (val < -headroom)
                                val = headroom - val - 1;
                            else
                                val = -1 - (val << 1);
                        else if (val >= headroom)
                            val = val + headroom;
                        else
                            val <<= 1;

                        output[i] = val;
                        post[ln] &= 0x7fff;
                        post[hn] &= 0x7fff;
                    }
                }

                // we have everything we need. pack it output 
                // mark nontrivial floor 
                buffer.Write(1, 1);

                // beginning/end post 
                var encodedQ = Encoding.Log(_quantQ - 1);
                buffer.Write((uint) output[0], encodedQ);
                buffer.Write((uint) output[1], encodedQ);


                // partition by partition 
                for (int i = 0, j = 2; i < _floor.PartitionClass.Length; i++)
                {
                    var c = _floor.PartitionClass[i];
                    var cdim = _floor.ClassDimensions[c];
                    var csubbits = _floor.ClassSubs[c];
                    var csub = 1 << csubbits;
                    var cval = 0;
                    var cshift = 0;
                    int k;
                    int[] bookas = {0, 0, 0, 0, 0, 0, 0, 0};

                    // generate the partition's first stage cascade value 
                    if (csubbits != 0)
                    {
                        var maxval = new int[8];
                        for (k = 0; k < csub; k++)
                        {
                            var bookNumber = _floor.ClassSubBook[c][k];
                            if (bookNumber < 0)
                                maxval[k] = 1;
                            else
                                maxval[k] = staticBooks[_floor.ClassSubBook[c][k]].LengthList.Length;
                        }
                        for (k = 0; k < cdim; k++)
                        {
                            for (var l = 0; l < csub; l++)
                            {
                                var val = output[j + k];
                                if (val < maxval[l])
                                {
                                    bookas[k] = l;
                                    break;
                                }
                            }
                            cval |= bookas[k] << cshift;
                            cshift += csubbits;
                        }

                        // write it 
                        buffer.WriteBook(books[_floor.ClassBook[c]], cval);
                    }

                    // write post values 
                    for (k = 0; k < cdim; k++)
                    {
                        var book = _floor.ClassSubBook[c][bookas[k]];
                        if (book >= 0)
                            if (output[j + k] < books[book].Entries)
                                buffer.WriteBook(books[book], output[j + k]);
                    }

                    j += cdim;
                }

                // generate quantized floor equivalent to what we'd unpack in decode 
                // render the lines 
                var hx = 0;
                var lx = 0;
                var ly = post[0]*_floor.Mult;

                for (var j = 1; j < _posts; j++)
                {
                    var current = _forwardIndex[j];
                    var hy = post[current] & 0x7fff;
                    if (hy == post[current])
                    {
                        hy *= _floor.Mult;
                        hx = _floor.PostList[current];

                        RenderLine0(n, lx, hx, ly, hy, ilogmask);

                        lx = hx;
                        ly = hy;
                    }
                }
                for (var j = hx; j < pcmEnd/2; j++) ilogmask[j] = ly; // be certain 
                return true;
            }

            buffer.Write(0, 1);
            Array.Clear(ilogmask, 0, pcmEnd/2);
            return false;
        }

        private void RenderLine0(int n, int x0, int x1, int y0, int y1, int[] d)
        {
            var dy = y1 - y0;
            var adx = x1 - x0;
            var ady = Math.Abs(dy);
            var b = dy/adx;
            var sy = dy < 0 ? b - 1 : b + 1;
            var x = x0;
            var y = y0;
            var err = 0;

            ady -= Math.Abs(b*adx);

            if (n > x1)
                n = x1;

            if (x < n)
                d[x] = y;

            while (++x < n)
            {
                err = err + ady;
                if (err >= adx)
                {
                    err -= adx;
                    y += sy;
                }
                else
                {
                    y += b;
                }
                d[x] = y;
            }
        }

        private struct FitAccumulation
        {
            public int x0;
            public int x1;
            public int xa;
            public int ya;
            public int x2a;
            public int xya;
            public int an;
            public int xb;
            public int yb;
            public int x2b;
            public int xyb;
            public int bn;
        }
    }
}