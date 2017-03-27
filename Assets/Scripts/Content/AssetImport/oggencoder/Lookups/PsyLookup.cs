using System;
using System.Collections.Generic;
using System.Linq;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{
    public class PsyLookup
    {
        private readonly float[] _ath;
        private readonly int[] _bark;
        private readonly int _eighthOctaveLines;
        private readonly int _firstOctave;
        private readonly float _mVal;
        private readonly int _n;
        private readonly float[][] _noiseOffset;
        private readonly int[] _octave;
        private readonly PsyInfo _psyInfo;
        private readonly int _shiftOctave;

        private readonly float[][][] _toneCurves;
        private readonly int _totalOctaveLines;

        public PsyLookup(PsyInfo psyInfo, PsyGlobal globalParam, int n, int sampleRate)
        {
            _psyInfo = psyInfo;
            _n = n;
            _ath = new float[n];
            _octave = new int[n];
            _bark = new int[n];

            int lo = -99, hi = 1;

            _eighthOctaveLines = globalParam.EighthOctaveLines;
            _shiftOctave = (int) Math.Round(Math.Log(globalParam.EighthOctaveLines*8f)/Math.Log(2)) - 1;

            _firstOctave = (int) (ToOctave(.25f*sampleRate*.5/n)
                                  *(1 << (_shiftOctave + 1))
                                  - globalParam.EighthOctaveLines);

            var maxOctave = (int) (ToOctave((n + .25f)*sampleRate*.5/n)
                                   *(1 << (_shiftOctave + 1))
                                   + .5f);

            _totalOctaveLines = maxOctave - _firstOctave + 1;

            // AoTuV HF weighting 
            _mVal = 1;
            if (sampleRate < 26000)
                _mVal = 0;
            else if (sampleRate < 38000)
                _mVal = .94f; // 32kHz 
            else if (sampleRate > 46000)
                _mVal = 1.275f; // 48kHz

            // set up the lookups for a given blocksize and sample rate
            int k, j;
            for (k = 0, j = 0; k < MaxAth - 1; k++)
            {
                var endPos = (int) Math.Round(FromOctave((k + 1)*.125 - 2)*2.0*n/sampleRate);
                var baseAth = AthSource[k];
                if (j < endPos)
                {
                    var delta = (AthSource[k + 1] - baseAth)/(endPos - j);
                    for (; (j < endPos) && (j < n); j++)
                    {
                        _ath[j] = baseAth + 100;
                        baseAth += delta;
                    }
                }
            }

            for (; j < n; j++)
                _ath[j] = _ath[j - 1];

            for (var i = 0; i < n; i++)
            {
                var bark = ToBark(sampleRate/(2*n)*i);

                for (;
                    (lo + psyInfo.NoiseWindowLowMin < i)
                    && (ToBark(sampleRate/(2*n)*lo) < bark - psyInfo.NoiseWindowLow);
                    lo++)
                {
                }

                for (;
                    (hi <= n) && ((hi < i + psyInfo.NoiseWindowHighMin)
                                  || (ToBark(sampleRate/(2*n)*hi) < bark + psyInfo.NoiseWindowHigh));
                    hi++)
                {
                }

                _bark[i] = ((lo - 1) << 16) + (hi - 1);
            }

            for (var i = 0; i < n; i++)
                _octave[i] = (int) (ToOctave((i + .25f)*.5*sampleRate/n)
                                    *(1 << (_shiftOctave + 1))
                                    + .5f);

            _toneCurves = SetupToneCurves(
                psyInfo.ToneAtt,
                sampleRate*.5/n,
                n,
                psyInfo.ToneCenterBoost,
                psyInfo.ToneDecay);

            /* set up rolling noise median */
            _noiseOffset = new float[_psyInfo.NoiseOffset.Length][];
            for (var i = 0; i < _noiseOffset.Length; i++)
                _noiseOffset[i] = new float[n];

            for (var i = 0; i < n; i++)
            {
                var halfOctave = ToOctave((i + .5)*sampleRate/(2.0*n))*2.0;

                if (halfOctave < 0)
                    halfOctave = 0;

                if (halfOctave >= PsyInfo.Bands - 1)
                    halfOctave = PsyInfo.Bands - 1;

                var intHalfOctave = (int) halfOctave;
                var del = halfOctave - intHalfOctave;

                for (var h = 0; h < _noiseOffset.Length; h++)
                    _noiseOffset[h][i] = (float) (psyInfo.NoiseOffset[h][intHalfOctave]*(1 - del)
                                                  + (del > 0 ? psyInfo.NoiseOffset[h][intHalfOctave + 1]*del : 0));
            }
        }

        private static float[][][] SetupToneCurves(
            float[] curveAttDecibels,
            double binHertz,
            int n,
            float centerBoost,
            float centerDecayRate)
        {
            var bruteBuffer = new float[n];
            var ath = new float[EhmerMax];
            var workc = new float[PsyInfo.Bands][][];
            for (var i = 0; i < workc.Length; ++i)
                workc[i] = new float[Levels][];

            var athc = new float[Levels][];
            var newToneCurves = new float[PsyInfo.Bands][][];

            for (var i = 0; i < PsyInfo.Bands; i++)
            {
                /* we add back in the AthSource to avoid low level curves falling off to
                -infinity and unnecessarily cutting off high level curves in the
                curve limiting (last step). */

                // A half-band's settings must be valid over the whole band, and it's better to mask too little than too much 
                var athOffset = i*4;
                for (var j = 0; j < EhmerMax; j++)
                {
                    var min = 999f;
                    for (var k = 0; k < 4; k++)
                        if (j + k + athOffset < MaxAth)
                        {
                            if (min > AthSource[j + k + athOffset])
                                min = AthSource[j + k + athOffset];
                        }
                        else
                        {
                            if (min > AthSource[MaxAth - 1])
                                min = AthSource[MaxAth - 1];
                        }

                    ath[j] = min;
                }

                // copy curves into working space, replicate the 50dB curve to 30 and 40, replicate the 100dB curve to 110 
                for (var j = 0; j < 6; j++)
                    workc[i][j + 2] = ToneMasks[i][j].ToArray().ToFixedLength(EhmerMax);

                workc[i][0] = ToneMasks[i][0].ToArray().ToFixedLength(EhmerMax);
                workc[i][1] = ToneMasks[i][0].ToArray().ToFixedLength(EhmerMax);

                /* apply centered curve boost/decay */
                for (var j = 0; j < Levels; j++)
                    for (var k = 0; k < EhmerMax; k++)
                    {
                        var adj = centerBoost + Math.Abs(EhmerOffset - k)*centerDecayRate;

                        if ((adj < 0) && (centerBoost > 0))
                            adj = 0;

                        if ((adj > 0) && (centerBoost < 0))
                            adj = 0;

                        workc[i][j][k] += adj;
                    }

                /* normalize curves so the driving amplitude is 0dB */
                /* make temp curves with the AthSource overlayed */
                for (var j = 0; j < Levels; j++)
                {
                    AttenuateCurve(
                        workc[i][j],
                        curveAttDecibels[i] + 100f - (j < 2 ? 2 : j)*10f - Level0);

                    athc[j] = ath.ToArray().ToFixedLength(EhmerMax);

                    AttenuateCurve(
                        athc[j],
                        100f - j*10f - Level0);

                    MaxCurve(athc[j], new List<float>(workc[i][j]));
                }

                /* Now limit the louder curves.
    
                the idea is this: We don't know what the playback attenuation
                will be; 0dB SL moves every time the user twiddles the volume
                knob. So that means we have to use a single 'most pessimal' curve
                for all masking amplitudes, right?  Wrong.  The *loudest* sound
                can be in (we assume) a range of ...+100dB] SL.  However, sounds
                20dB down will be in a range ...+80], 40dB down is from ...+60],
                etc... */

                for (var j = 1; j < Levels; j++)
                {
                    MinCurve(athc[j], new List<float>(athc[j - 1]));
                    MinCurve(workc[i][j], new List<float>(athc[j]));
                }
            }

            for (var i = 0; i < PsyInfo.Bands; i++)
            {
                newToneCurves[i] = new float[Levels][];

                /* low frequency curves are measured with greater resolution than
                the MDCT/FFT will actually give us; we want the curve applied
                to the tone data to be pessimistic and thus apply the minimum
                masking possible for a given bin.  That means that a single bin
                could span more than one octave and that the curve will be a
                composite of multiple octaves.  It also may mean that a single
                bin may span > an eighth of an octave and that the eighth
                octave values may also be composited. */

                /* which octave curves will we be compositing? */
                var bin = (int) Math.Floor(FromOctave(i*.5)/binHertz);
                var lowCurve = (int) Math.Ceiling(ToOctave(bin*binHertz + 1)*2);
                var highCurve = (int) Math.Floor(ToOctave((bin + 1)*binHertz)*2);
                if (lowCurve > i) lowCurve = i;
                if (lowCurve < 0) lowCurve = 0;
                if (highCurve >= PsyInfo.Bands) highCurve = PsyInfo.Bands - 1;

                for (var m = 0; m < Levels; m++)
                {
                    newToneCurves[i][m] = new float[EhmerMax + 2];

                    for (var j = 0; j < n; j++)
                        bruteBuffer[j] = 999;

                    // Render the curve into bins, then pull values back into curve. 
                    // The point is that any inherent subsampling aliasing results in a safe minimum
                    for (var k = lowCurve; k <= highCurve; k++)
                    {
                        var l = 0;
                        for (var j = 0; j < EhmerMax; j++)
                        {
                            var lowBin = (int) (FromOctave(j*.125 + k*.5 - 2.0625)/binHertz);
                            var highBin = FromOctave(j*.125 + k*.5 - 1.9375)/binHertz + 1;

                            if (lowBin < 0)
                                lowBin = 0;

                            if (lowBin > n)
                                lowBin = n;

                            if (lowBin < l)
                                l = lowBin;

                            if (highBin < 0)
                                highBin = 0;

                            if (highBin > n)
                                highBin = n;

                            for (; (l < highBin) && (l < n); l++)
                                if (bruteBuffer[l] > workc[k][m][j])
                                    bruteBuffer[l] = workc[k][m][j];
                        }

                        for (; l < n; l++)
                            if (bruteBuffer[l] > workc[k][m][EhmerMax - 1])
                                bruteBuffer[l] = workc[k][m][EhmerMax - 1];
                    }

                    if (i + 1 < PsyInfo.Bands)
                    {
                        var l = 0;
                        var k = i + 1;
                        for (var j = 0; j < EhmerMax; j++)
                        {
                            var lowBin = (int) (FromOctave(j*.125 + i*.5 - 2.0625)/binHertz);
                            var highBin = (int) (FromOctave(j*.125 + i*.5 - 1.9375)/binHertz + 1);

                            if (lowBin < 0)
                                lowBin = 0;

                            if (lowBin > n)
                                lowBin = n;

                            if (lowBin < l)
                                l = lowBin;

                            if (highBin < 0)
                                highBin = 0;

                            if (highBin > n)
                                highBin = n;

                            for (; (l < highBin) && (l < n); l++)
                                if (bruteBuffer[l] > workc[k][m][j])
                                    bruteBuffer[l] = workc[k][m][j];
                        }

                        for (; l < n; l++)
                            if (bruteBuffer[l] > workc[k][m][EhmerMax - 1])
                                bruteBuffer[l] = workc[k][m][EhmerMax - 1];
                    }


                    for (var j = 0; j < EhmerMax; j++)
                    {
                        var bufferBin = (int) (FromOctave(j*.125 + i*.5 - 2.0)/binHertz);
                        if (bufferBin < 0)
                        {
                            newToneCurves[i][m][j + 2] = -999;
                        }
                        else
                        {
                            if (bufferBin >= n)
                                newToneCurves[i][m][j + 2] = -999;
                            else
                                newToneCurves[i][m][j + 2] = bruteBuffer[bufferBin];
                        }
                    }

                    /* add fenceposts */
                    int e;
                    for (e = 0; e < EhmerOffset; e++)
                        if (newToneCurves[i][m][e + 2] > -200f)
                            break;

                    newToneCurves[i][m][0] = e;

                    for (e = EhmerMax - 1; e > EhmerOffset + 1; e--)
                        if (newToneCurves[i][m][e + 2] > -200f)
                            break;

                    newToneCurves[i][m][1] = e;
                }
            }

            return newToneCurves;
        }

        public void ToneMask(
            IList<float> pcm,
            float[] logmask,
            float globalSpecMax,
            float localSpecMax)
        {
            var seed = new float[_totalOctaveLines];
            var att = localSpecMax + _psyInfo.AthAdjAtt;

            for (var i = 0; i < seed.Length; i++)
                seed[i] = NegativeInfinite;

            // set the ATH (floating below localmax, not global max by a specified att)
            if (att < _psyInfo.AthMaxAtt)
                att = _psyInfo.AthMaxAtt;

            for (var i = 0; i < _n; i++)
                logmask[i] = _ath[i] + att;

            // tone masking 
            SeedLoop(_toneCurves, pcm, logmask, seed, globalSpecMax);
            MaxSeeds(seed, logmask);
        }

        public void OffsetAndMix(
            List<float> noise,
            List<float> tone,
            int offsetIndex,
            IList<float> logmask,
            IList<float> mdct,
            IList<float> logmdct)
        {
            var toneAtt = _psyInfo.ToneMasterAtt[offsetIndex];

            double cx = _mVal;

            for (var i = 0; i < _n; i++)
            {
                var val = noise[i] + _noiseOffset[offsetIndex][i];

                if (val > _psyInfo.NoiseMaxSuppress)
                    val = _psyInfo.NoiseMaxSuppress;

                logmask[i] = Math.Max(val, tone[i] + toneAtt);

                // AoTuV 
                // @ M1 **
                //  The following codes improve a noise problem.
                //  A fundamental idea uses the value of masking and carries out
                //  the relative compensation of the MDCT.
                //  However, this code is not perfect and all noise problems cannot be solved.
                //  by Aoyumi @ 2004/04/18

                if (offsetIndex == 1)
                {
                    double coeffi = -17.2f; // AoTuV
                    val = val - logmdct[i]; // val == mdct line value relative to floor in dB 

                    double de;
                    if (val > coeffi)
                    {
                        // mdct value is > -17.2 dB below floor 

                        de = 1.0 - (val - coeffi)*0.005*cx;

                        // pro-rated attenuation:
                        // -0.00 dB boost if mdct value is -17.2dB (relative to floor)
                        // -0.77 dB boost if mdct value is 0dB (relative to floor)
                        // -1.64 dB boost if mdct value is +17.2dB (relative to floor)
                        // etc... 
                        if (de < 0)
                            de = 0.0001f;
                    }
                    else
                    {
                        // mdct value is <= -17.2 dB below floor 
                        de = 1.0 - (val - coeffi)*0.0003*cx;
                    }

                    // pro-rated attenuation:
                    //  +0.00 dB atten if mdct value is -17.2dB (relative to floor)
                    //  +0.45 dB atten if mdct value is -34.4dB (relative to floor)
                    //  etc... 
                    mdct[i] *= (float) de;
                }
            }
        }

        public void NoiseMask(IList<float> logmdct, float[] logmask)
        {
            var work = new float[_n];

            BarkNoiseHybridMp(logmdct, logmask, 140, -1);

            for (var i = 0; i < _n; i++)
                work[i] = logmdct[i] - logmask[i];

            BarkNoiseHybridMp(work, logmask, 0, _psyInfo.NoiseWindowFixed);

            for (var i = 0; i < _n; i++)
                work[i] = logmdct[i] - work[i];

            for (var i = 0; i < _n; i++)
            {
                var dB = (int) (logmask[i] + .5);

                if (dB >= _psyInfo.NoiseCompand.Length)
                    dB = _psyInfo.NoiseCompand.Length - 1;

                if (dB < 0)
                    dB = 0;

                logmask[i] = work[i] + _psyInfo.NoiseCompand[dB];
            }
        }

        private void BarkNoiseHybridMp(
            IList<float> f,
            IList<float> noise,
            float offset,
            int adjusted)
        {
            var fp = new float[_n];
            var fx = new float[_n];
            var fxx = new float[_n];
            var fy = new float[_n];
            var fxy = new float[_n];

            int i;

            int lo, hi;
            float r;
            var a = 0f;
            var b = 0f;
            var d = 1f;
            float x;

            var tn = 0f;
            var tx = 0f;
            var txx = 0f;
            var ty = 0f;
            var txy = 0f;

            var y = f[0] + offset;
            if (y < 1f)
                y = 1f;

            var w = (float) (y*y*.5);

            tn += w;
            tx += w;
            ty += w*y;

            fp[0] = tn;
            fx[0] = tx;
            fxx[0] = txx;
            fy[0] = ty;
            fxy[0] = txy;

            for (i = 1, x = 1f; i < _n; i++, x += 1f)
            {
                y = f[i] + offset;
                if (y < 1f) y = 1f;

                w = y*y;

                tn += w;
                tx += w*x;
                txx += w*x*x;
                ty += w*y;
                txy += w*x*y;

                fp[i] = tn;
                fx[i] = tx;
                fxx[i] = txx;
                fy[i] = ty;
                fxy[i] = txy;
            }

            for (i = 0, x = 0f;; i++, x += 1f)
            {
                lo = _bark[i] >> 16;
                if (lo >= 0)
                    break;

                hi = _bark[i] & 0xffff;

                tn = fp[hi] + fp[-lo];
                tx = fx[hi] - fx[-lo];
                txx = fxx[hi] + fxx[-lo];
                ty = fy[hi] + fy[-lo];
                txy = fxy[hi] - fxy[-lo];

                a = ty*txx - tx*txy;
                b = tn*txy - tx*ty;
                d = tn*txx - tx*tx;
                r = (a + x*b)/d;
                if (r < 0f)
                    r = 0f;

                noise[i] = r - offset;
            }

            for (;; i++, x += 1f)
            {
                lo = _bark[i] >> 16;
                hi = _bark[i] & 0xffff;
                if (hi >= _n)
                    break;

                tn = fp[hi] - fp[lo];
                tx = fx[hi] - fx[lo];
                txx = fxx[hi] - fxx[lo];
                ty = fy[hi] - fy[lo];
                txy = fxy[hi] - fxy[lo];

                a = ty*txx - tx*txy;
                b = tn*txy - tx*ty;
                d = tn*txx - tx*tx;
                r = (a + x*b)/d;
                if (r < 0f) r = 0f;

                noise[i] = r - offset;
            }
            for (; i < _n; i++, x += 1f)
            {
                r = (a + x*b)/d;
                if (r < 0f) r = 0f;

                noise[i] = r - offset;
            }

            if (adjusted <= 0) return;

            for (i = 0, x = 0f;; i++, x += 1f)
            {
                hi = i + adjusted/2;
                lo = hi - adjusted;
                if (lo >= 0) break;

                tn = fp[hi] + fp[-lo];
                tx = fx[hi] - fx[-lo];
                txx = fxx[hi] + fxx[-lo];
                ty = fy[hi] + fy[-lo];
                txy = fxy[hi] - fxy[-lo];


                a = ty*txx - tx*txy;
                b = tn*txy - tx*ty;
                d = tn*txx - tx*tx;
                r = (a + x*b)/d;

                if (r - offset < noise[i]) noise[i] = r - offset;
            }
            for (;; i++, x += 1f)
            {
                hi = i + adjusted/2;
                lo = hi - adjusted;
                if (hi >= _n) break;

                tn = fp[hi] - fp[lo];
                tx = fx[hi] - fx[lo];
                txx = fxx[hi] - fxx[lo];
                ty = fy[hi] - fy[lo];
                txy = fxy[hi] - fxy[lo];

                a = ty*txx - tx*txy;
                b = tn*txy - tx*ty;
                d = tn*txx - tx*tx;
                r = (a + x*b)/d;

                if (r - offset < noise[i]) noise[i] = r - offset;
            }
            for (; i < _n; i++, x += 1f)
            {
                r = (a + x*b)/d;
                if (r - offset < noise[i]) noise[i] = r - offset;
            }
        }

        public void CoupleQuantizeNormalize(
            int blobno,
            PsyGlobal psyGlobal,
            Mapping mapping,
            List<float[]> mdct,
            List<int[]> iwork,
            bool[] nonzero,
            int slidingLowpass,
            int channels)
        {
            var partition = _psyInfo.Normalize ? _psyInfo.NormalPartition : 1;
            var limit = psyGlobal.CouplingPointLimit[_psyInfo.BlockFlag][blobno];
            var prepoint = StereoThresholds[psyGlobal.CouplingPrePointAmp[blobno]];
            var postpoint = StereoThresholds[psyGlobal.CouplingPostPointAmp[blobno]];

            // non-zero flag working vector 
            var nz = new bool[channels];

            // energy surplus/deficit tracking 
            var acc = new float[channels + mapping.CouplingSteps];

            // The threshold of a stereo is changed with the size of n 
            if (_n > 1000)
                postpoint = StereoThresholdsLimit[psyGlobal.CouplingPostPointAmp[blobno]];

            // mdct is our raw mdct output, floor not removed. 
            // inout passes in the ifloor, passes back quantized result 

            // unquantized energy (negative indicates amplitude has negative sign) 
            var raw = new float[channels*partition];

            // dual pupose; quantized energy (if flag set), othersize fabs(raw) 
            var quant = new float[channels*partition];

            // floor energy 
            var floor = new float[channels*partition];

            // flags indicating raw/quantized status of elements in raw vector 
            var flag = new bool[channels*partition];

            for (var i = 0; i < channels + mapping.CouplingSteps; i++)
                acc[i] = 0f;

            for (var i = 0; i < _n; i += partition)
            {
                var jn = partition > _n - i ? _n - i : partition;
                int step, track = 0;

                // prefill 
                for (var channel = 0; channel < channels; channel++)
                {
                    nz[channel] = nonzero[channel];
                    if (nz[channel])
                    {
                        for (var j = 0; j < jn; j++)
                            floor[channel*partition + j] = DecibelLookup[iwork[channel][i + j]];

                        FlagLossless(limit, prepoint, postpoint, mdct[channel], floor, flag, channel*partition, i, jn);

                        for (var j = 0; j < jn; j++)
                        {
                            var index = channel*partition + j;

                            quant[index] = raw[index] = mdct[channel][i + j]*mdct[channel][i + j];

                            if (mdct[channel][i + j] < 0f)
                                raw[index] *= -1f;

                            floor[index] *= floor[channel*partition + j];
                        }

                        acc[track] = NoiseNormalize(
                            limit,
                            raw,
                            quant,
                            floor,
                            null,
                            channel*partition,
                            i,
                            jn,
                            iwork[channel]);
                    }
                    else
                    {
                        for (var j = 0; j < jn; j++)
                        {
                            var index = channel*partition + j;
                            floor[index] = 1e-10f;
                            raw[index] = 0f;
                            quant[index] = 0f;
                            flag[index] = false;
                            iwork[channel][i + j] = 0;
                        }

                        acc[track] = 0f;
                    }
                    track++;
                }

                // coupling 
                for (step = 0; step < mapping.CouplingSteps; step++)
                {
                    var mag = mapping.CouplingMag[step];
                    var ang = mapping.CouplingAng[step];

                    if (nz[mag] || nz[ang])
                    {
                        nz[mag] = nz[ang] = true;

                        for (var j = 0; j < jn; j++)
                        {
                            if (j < slidingLowpass - i)
                                if (flag[mag*partition + j] || flag[ang*partition + j])
                                {
                                    // lossless coupling 
                                    raw[mag*partition + j] = Math.Abs(raw[mag*partition + j])
                                                             + Math.Abs(raw[ang*partition + j]);

                                    quant[mag*partition + j] = quant[mag*partition + j]
                                                               + quant[ang*partition + j];

                                    flag[mag*partition + j] = flag[ang*partition + j] = true;

                                    // couple iM/iA 
                                    {
                                        var a = iwork[mag][i + j];
                                        var b = iwork[ang][i + j];

                                        if (Math.Abs(a) > Math.Abs(b))
                                        {
                                            iwork[ang][i + j] = a > 0 ? a - b : b - a;
                                        }
                                        else
                                        {
                                            iwork[ang][i + j] = b > 0 ? a - b : b - a;
                                            iwork[mag][i + j] = b;
                                        }

                                        // collapse two equivalent tuples to one 
                                        if (iwork[ang][i + j] >= Math.Abs(iwork[mag][i + j])*2)
                                        {
                                            iwork[ang][i + j] = -iwork[ang][i + j];
                                            iwork[mag][i + j] = -iwork[mag][i + j];
                                        }
                                    }
                                }
                                else
                                {
                                    // lossy (point) coupling 
                                    if (j < limit - i)
                                    {
                                        // dipole 
                                        raw[mag*partition + j] += raw[ang*partition + j];
                                        quant[mag*partition + j] = Math.Abs(raw[mag*partition + j]);
                                    }
                                    else
                                    {
                                        // elliptical 
                                        if (raw[mag*partition + j] + raw[ang*partition + j] < 0)
                                            raw[mag*partition + j] =
                                                -(quant[mag*partition + j] =
                                                    Math.Abs(raw[mag*partition + j]) + Math.Abs(raw[ang*partition + j]));
                                        else
                                            raw[mag*partition + j] =
                                                quant[mag*partition + j] =
                                                    Math.Abs(raw[mag*partition + j]) + Math.Abs(raw[ang*partition + j]);
                                    }

                                    raw[ang*partition + j] = quant[ang*partition + j] = 0f;
                                    flag[ang*partition + j] = true;
                                    iwork[ang][i + j] = 0;
                                }
                            floor[mag*partition + j] =
                                floor[ang*partition + j] = floor[mag*partition + j] + floor[ang*partition + j];
                        }

                        // normalize the resulting mag vector 
                        acc[track] = NoiseNormalize(
                            limit,
                            raw,
                            quant,
                            floor,
                            flag,
                            mag,
                            i,
                            jn,
                            iwork[mag]);

                        track++;
                    }
                }
            }
        }

        private float NoiseNormalize(
            int limit,
            IList<float> r,
            IList<float> q,
            IList<float> f,
            IList<bool> flags,
            int offset,
            int i,
            int n,
            IList<int> output)
        {
            var sort = new List<OffsetArray<float>>(n);

            int j;
            var start = _psyInfo.Normalize ? _psyInfo.NormalStart - i : n;
            if (start > n)
                start = n;

            // force classic behavior where only energy in the current band is considered 
            var acc = 0f;

            // still responsible for populating *output where noise norm not in
            // effect.  There's no need to [re]populate *q in these areas 
            for (j = 0; j < start; j++)
            {
                if ((flags != null) && flags[offset + j])
                    continue;

                // Lossless coupling already quantized.
                // Don't touch; requantizing based on
                // energy would be incorrect.
                var ve = q[offset + j]/f[offset + j];
                if (r[offset + j] < 0)
                    output[i + j] = (int) -Math.Round(Math.Sqrt(ve));
                else
                    output[i + j] = (int) Math.Round(Math.Sqrt(ve));
            }

            // sort magnitudes for noise norm portion of partition 
            for (; j < n; j++)
                if ((flags == null) || !flags[offset + j])
                {
                    // can't noise norm elements that have
                    // already been losslessly coupled; we can
                    // only account for their energy error
                    var ve = q[offset + j]/f[offset + j];

                    // Despite all the new, more capable coupling code, for now we
                    // implement noise norm as it has been up to this point. Only
                    // consider promotions to unit magnitude from 0.  In addition
                    // the only energy error counted is quantizations to zero.
                    // also-- the original point code only applied noise norm at > pointlimit
                    if ((ve < .25f) && ((flags == null) || (j >= limit - i)))
                    {
                        acc += ve;
                        sort.Add(new OffsetArray<float>(q, j)); // q is fabs(r) for unflagged element
                    }
                    else
                    {
                        // For now: no acc adjustment for nonzero quantization.  populate *output and q as this value is final.
                        if (r[offset + j] < 0)
                            output[i + j] = (int) -Math.Round(Math.Sqrt(ve));
                        else
                            output[i + j] = (int) Math.Round(Math.Sqrt(ve));

                        q[offset + j] = output[i + j]*output[i + j]*f[offset + j];
                    }
                }

            if (sort.Count > 0)
            {
                sort.Sort(Comparer);

                // noise norm to do 
                for (j = 0; j < sort.Count; j++)
                {
                    var k = sort[j].Offset;
                    if (acc >= _psyInfo.NormalThreshold)
                    {
                        output[i + k] = (int) UnitNorm(r[offset + k]);
                        acc -= 1f;
                        q[offset + k] = f[offset + k];
                    }
                    else
                    {
                        output[i + k] = 0;
                        q[offset + k] = 0f;
                    }
                }
            }

            return acc;
        }

        private static void FlagLossless(
            int limit, float prePoint, float postPoint, IList<float> mdct,
            IList<float> floor, IList<bool> flag, int offset, int i, int jn)
        {
            for (var j = 0; j < jn; j++)
            {
                var point = j >= limit - i ? postPoint : prePoint;
                var r = Math.Abs(mdct[j + i])/floor[offset + j];
                flag[offset + j] = r >= point;
            }
        }

        private static float UnitNorm(float x)
        {
            var i = BitConverter.ToUInt32(BitConverter.GetBytes(x), 0);
            i = (i & 0x80000000U) | 0x3f800000U;
            return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
        }

        private void MaxSeeds(float[] seed, IList<float> floor)
        {
            var linePosition = 0;

            SeedChase(seed); // for masking

            var pos = _octave[0] - _firstOctave - (_eighthOctaveLines >> 1);

            float minV;
            while (linePosition + 1 < _n)
            {
                minV = seed[pos];
                var end = ((_octave[linePosition] + _octave[linePosition + 1]) >> 1) - _firstOctave;

                if (minV > _psyInfo.ToneAbsLimit)
                    minV = _psyInfo.ToneAbsLimit;

                while (pos + 1 <= end)
                {
                    pos++;
                    if (((seed[pos] > NegativeInfinite) && (seed[pos] < minV)) || (minV <= NegativeInfinite))
                        minV = seed[pos];
                }

                end = pos + _firstOctave;
                for (; (linePosition < _n) && (_octave[linePosition] <= end); linePosition++)
                    if (floor[linePosition] < minV)
                        floor[linePosition] = minV;
            }

            minV = seed[_totalOctaveLines - 1];
            for (; linePosition < _n; linePosition++)
                if (floor[linePosition] < minV)
                    floor[linePosition] = minV;
        }

        private void SeedChase(IList<float> seeds)
        {
            var posStack = new int[_totalOctaveLines];
            var ampStack = new float[_totalOctaveLines];
            var stack = 0;
            var pos = 0;

            for (var i = 0; i < _totalOctaveLines; i++)
                if (stack < 2)
                {
                    posStack[stack] = i;
                    ampStack[stack++] = seeds[i];
                }
                else
                {
                    while (true)
                        if (seeds[i] < ampStack[stack - 1])
                        {
                            posStack[stack] = i;
                            ampStack[stack++] = seeds[i];
                            break;
                        }
                        else
                        {
                            if (i < posStack[stack - 1] + _eighthOctaveLines)
                                if ((stack > 1) && (ampStack[stack - 1] <= ampStack[stack - 2]) &&
                                    (i < posStack[stack - 2] + _eighthOctaveLines))
                                {
                                    // we completely overlap, making stack-1 irrelevant.  pop it 
                                    stack--;
                                    continue;
                                }
                            posStack[stack] = i;
                            ampStack[stack++] = seeds[i];
                            break;
                        }
                }

            // the stack now contains only the positions that are relevant. Scan 'em straight through 
            for (var i = 0; i < stack; i++)
            {
                int endPosition;
                if ((i < stack - 1) && (ampStack[i + 1] > ampStack[i]))
                    endPosition = posStack[i + 1];
                else
                    endPosition = posStack[i] + _eighthOctaveLines + 1;

                if (endPosition > _totalOctaveLines) endPosition = _totalOctaveLines;
                for (; pos < endPosition; pos++)
                    seeds[pos] = ampStack[i];
            }
        }

        private void SeedLoop(float[][][] curves, IList<float> pcm, float[] floor, float[] seeds, float specmax)
        {
            // prime the working vector with peak values
            for (var i = 0; i < _n; i++)
            {
                var max = pcm[i];
                var oc = _octave[i];
                while ((i + 1 < _n) && (_octave[i + 1] == oc))
                {
                    i++;
                    if (pcm[i] > max)
                        max = pcm[i];
                }

                if (max + 6f > floor[i])
                {
                    oc = oc >> _shiftOctave;

                    if (oc >= PsyInfo.Bands)
                        oc = PsyInfo.Bands - 1;

                    if (oc < 0)
                        oc = 0;

                    SeedCurve(
                        seeds,
                        curves[oc],
                        max,
                        _octave[i] - _firstOctave,
                        _totalOctaveLines,
                        _eighthOctaveLines,
                        _psyInfo.MaxCurveDecibel - specmax);
                }
            }
        }

        private static void SeedCurve(IList<float> seeds, float[][] curves, float amp, int oc, int n, int linesper,
            float dbOffset)
        {
            var choice = (int) ((amp + dbOffset - Level0)*.1f);
            choice = Math.Max(choice, 0);
            choice = Math.Min(choice, Levels - 1);
            var posts = curves[choice];

            var post1 = (int) posts[1];
            var seedIndex = (int) (oc + (posts[0] - EhmerOffset)*linesper - (linesper >> 1));

            for (var i = (int) posts[0]; i < post1; i++)
            {
                if (seedIndex > 0)
                {
                    var lin = amp + posts[2 + i];

                    if (seeds[seedIndex] < lin)
                        seeds[seedIndex] = lin;
                }

                seedIndex += linesper;

                if (seedIndex >= n)
                    break;
            }
        }

        private static void MinCurve(IList<float> c1, List<float> c2)
        {
            for (var i = 0; i < EhmerMax; i++)
                if (c2[i] < c1[i])
                    c1[i] = c2[i];
        }

        private static void MaxCurve(IList<float> c1, List<float> c2)
        {
            for (var i = 0; i < EhmerMax; i++)
                if (c2[i] > c1[i])
                    c1[i] = c2[i];
        }

        private static void AttenuateCurve(IList<float> curve, float att)
        {
            for (var i = 0; i < EhmerMax; i++)
                curve[i] += att;
        }

        private static double ToOctave(double n)
        {
            return Math.Log(n) * 1.442695f - 5.965784f;
        }

        private static double FromOctave(double n)
        {
            return Math.Exp((n + 5.965784f) * .693147f);
        }

        private static double ToBark(double n)
        {
            return 13.1f * Math.Atan(.00074f * n) + 2.24f * Math.Atan(n * n * 1.85e-8f) + 1e-4f * n;
        }

        private class ApComparer : IComparer<OffsetArray<float>>
        {
            public int Compare(OffsetArray<float> x, OffsetArray<float> y)
            {
                var f1 = x[0];
                var f2 = y[0];
                return (f1 < f2 ? 1 : 0)
                       - (f1 > f2 ? 1 : 0);
            }
        }

        #region SOURCE DATA

        private static readonly List<float> StereoThresholdsLimit = new List<float>
            {0, .5f, 1, 1.5f, 2, 2.5f, 4.5f, 8.5f, 9e10f};

        private static readonly List<float> StereoThresholds = new List<float>
            {0, .5f, 1, 1.5f, 2.5f, 4.5f, 8.5f, 16.5f, 9e10f};

        private static readonly List<float> DecibelLookup = new List<float>
        {
            1.0649863e-07f, 1.1341951e-07f, 1.2079015e-07f, 1.2863978e-07f,
            1.3699951e-07f, 1.4590251e-07f, 1.5538408e-07f, 1.6548181e-07f,
            1.7623575e-07f, 1.8768855e-07f, 1.9988561e-07f, 2.128753e-07f,
            2.2670913e-07f, 2.4144197e-07f, 2.5713223e-07f, 2.7384213e-07f,
            2.9163793e-07f, 3.1059021e-07f, 3.3077411e-07f, 3.5226968e-07f,
            3.7516214e-07f, 3.9954229e-07f, 4.2550680e-07f, 4.5315863e-07f,
            4.8260743e-07f, 5.1396998e-07f, 5.4737065e-07f, 5.8294187e-07f,
            6.2082472e-07f, 6.6116941e-07f, 7.0413592e-07f, 7.4989464e-07f,
            7.9862701e-07f, 8.5052630e-07f, 9.0579828e-07f, 9.6466216e-07f,
            1.0273513e-06f, 1.0941144e-06f, 1.1652161e-06f, 1.2409384e-06f,
            1.3215816e-06f, 1.4074654e-06f, 1.4989305e-06f, 1.5963394e-06f,
            1.7000785e-06f, 1.8105592e-06f, 1.9282195e-06f, 2.0535261e-06f,
            2.1869758e-06f, 2.3290978e-06f, 2.4804557e-06f, 2.6416497e-06f,
            2.8133190e-06f, 2.9961443e-06f, 3.1908506e-06f, 3.3982101e-06f,
            3.6190449e-06f, 3.8542308e-06f, 4.1047004e-06f, 4.3714470e-06f,
            4.6555282e-06f, 4.9580707e-06f, 5.2802740e-06f, 5.6234160e-06f,
            5.9888572e-06f, 6.3780469e-06f, 6.7925283e-06f, 7.2339451e-06f,
            7.7040476e-06f, 8.2047000e-06f, 8.7378876e-06f, 9.3057248e-06f,
            9.9104632e-06f, 1.0554501e-05f, 1.1240392e-05f, 1.1970856e-05f,
            1.2748789e-05f, 1.3577278e-05f, 1.4459606e-05f, 1.5399272e-05f,
            1.6400004e-05f, 1.7465768e-05f, 1.8600792e-05f, 1.9809576e-05f,
            2.1096914e-05f, 2.2467911e-05f, 2.3928002e-05f, 2.5482978e-05f,
            2.7139006e-05f, 2.8902651e-05f, 3.0780908e-05f, 3.2781225e-05f,
            3.4911534e-05f, 3.7180282e-05f, 3.9596466e-05f, 4.2169667e-05f,
            4.4910090e-05f, 4.7828601e-05f, 5.0936773e-05f, 5.4246931e-05f,
            5.7772202e-05f, 6.1526565e-05f, 6.5524908e-05f, 6.9783085e-05f,
            7.4317983e-05f, 7.9147585e-05f, 8.4291040e-05f, 8.9768747e-05f,
            9.5602426e-05f, 0.00010181521f, 0.00010843174f, 0.00011547824f,
            0.00012298267f, 0.00013097477f, 0.00013948625f, 0.00014855085f,
            0.00015820453f, 0.00016848555f, 0.00017943469f, 0.00019109536f,
            0.00020351382f, 0.00021673929f, 0.00023082423f, 0.00024582449f,
            0.00026179955f, 0.00027881276f, 0.00029693158f, 0.00031622787f,
            0.00033677814f, 0.00035866388f, 0.00038197188f, 0.00040679456f,
            0.00043323036f, 0.00046138411f, 0.00049136745f, 0.00052329927f,
            0.00055730621f, 0.00059352311f, 0.00063209358f, 0.00067317058f,
            0.00071691700f, 0.00076350630f, 0.00081312324f, 0.00086596457f,
            0.00092223983f, 0.00098217216f, 0.0010459992f, 0.0011139742f,
            0.0011863665f, 0.0012634633f, 0.0013455702f, 0.0014330129f,
            0.0015261382f, 0.0016253153f, 0.0017309374f, 0.0018434235f,
            0.0019632195f, 0.0020908006f, 0.0022266726f, 0.0023713743f,
            0.0025254795f, 0.0026895994f, 0.0028643847f, 0.0030505286f,
            0.0032487691f, 0.0034598925f, 0.0036847358f, 0.0039241906f,
            0.0041792066f, 0.0044507950f, 0.0047400328f, 0.0050480668f,
            0.0053761186f, 0.0057254891f, 0.0060975636f, 0.0064938176f,
            0.0069158225f, 0.0073652516f, 0.0078438871f, 0.0083536271f,
            0.0088964928f, 0.009474637f, 0.010090352f, 0.010746080f,
            0.011444421f, 0.012188144f, 0.012980198f, 0.013823725f,
            0.014722068f, 0.015678791f, 0.016697687f, 0.017782797f,
            0.018938423f, 0.020169149f, 0.021479854f, 0.022875735f,
            0.024362330f, 0.025945531f, 0.027631618f, 0.029427276f,
            0.031339626f, 0.033376252f, 0.035545228f, 0.037855157f,
            0.040315199f, 0.042935108f, 0.045725273f, 0.048696758f,
            0.051861348f, 0.055231591f, 0.058820850f, 0.062643361f,
            0.066714279f, 0.071049749f, 0.075666962f, 0.080584227f,
            0.085821044f, 0.091398179f, 0.097337747f, 0.10366330f,
            0.11039993f, 0.11757434f, 0.12521498f, 0.13335215f,
            0.14201813f, 0.15124727f, 0.16107617f, 0.17154380f,
            0.18269168f, 0.19456402f, 0.20720788f, 0.22067342f,
            0.23501402f, 0.25028656f, 0.26655159f, 0.28387361f,
            0.30232132f, 0.32196786f, 0.34289114f, 0.36517414f,
            0.38890521f, 0.41417847f, 0.44109412f, 0.46975890f,
            0.50028648f, 0.53279791f, 0.56742212f, 0.60429640f,
            0.64356699f, 0.68538959f, 0.72993007f, 0.77736504f,
            0.82788260f, 0.88168307f, 0.9389798f, 1f
        };

        private static readonly ApComparer Comparer = new ApComparer();

        private const float NegativeInfinite = -9999f;
        private const int MaxAth = 88;
        private const int Levels = 8;
        private const int Level0 = 30;
        private const int EhmerMax = 56;
        private const int EhmerOffset = 16;

        private static readonly float[] AthSource =
        {
            /*15*/  -51f, -52, -53, -54, -55, -56, -57, -58,
            /*31*/  -59, -60, -61, -62, -63, -64, -65, -66,
            /*63*/  -67, -68, -69, -70, -71, -72, -73, -74,
            /*125*/ -75, -76, -77, -78, -80, -81, -82, -83,
            /*250*/ -84, -85, -86, -87, -88, -88, -89, -89,
            /*500*/ -90, -91, -91, -92, -93, -94, -95, -96,
            /*1k*/  -96, -97, -98, -98, -99, -99, -100, -100,
            /*2k*/ -101, -102, -103, -104, -106, -107, -107, -107,
            /*4k*/ -107, -105, -103, -102, -101, -99, -98, -96,
            /*8k*/  -95, -95, -96, -97, -96, -95, -93, -90,
            /*16k*/ -80, -70, -50, -40, -30, -30, -30, -30
        };

        private static readonly float[][][] ToneMasks =
        {
            /* 62.5 Hz */
            new[]
            {
                new float[]
                {
                    -60, -60, -60, -60, -60, -60, -60, -60,
                    -60, -60, -60, -60, -62, -62, -65, -73,
                    -69, -68, -68, -67, -70, -70, -72, -74,
                    -75, -79, -79, -80, -83, -88, -93, -100,
                    -110, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -48, -48, -48, -48, -48, -48, -48, -48,
                    -48, -48, -48, -48, -48, -53, -61, -66,
                    -66, -68, -67, -70, -76, -76, -72, -73,
                    -75, -76, -78, -79, -83, -88, -93, -100,
                    -110, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -37, -37, -37, -37, -37, -37, -37, -37,
                    -38, -40, -42, -46, -48, -53, -55, -62,
                    -65, -58, -56, -56, -61, -60, -65, -67,
                    -69, -71, -77, -77, -78, -80, -82, -84,
                    -88, -93, -98, -106, -112, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -25, -25, -25, -25, -25, -25, -25, -25,
                    -25, -26, -27, -29, -32, -38, -48, -52,
                    -52, -50, -48, -48, -51, -52, -54, -60,
                    -67, -67, -66, -68, -69, -73, -73, -76,
                    -80, -81, -81, -85, -85, -86, -88, -93,
                    -100, -110, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -16, -16, -16, -16, -16, -16, -16, -16,
                    -17, -19, -20, -22, -26, -28, -31, -40,
                    -47, -39, -39, -40, -42, -43, -47, -51,
                    -57, -52, -55, -55, -60, -58, -62, -63,
                    -70, -67, -69, -72, -73, -77, -80, -82,
                    -83, -87, -90, -94, -98, -104, -115, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -8, -8, -8, -8, -8, -8, -8, -8,
                    -8, -8, -10, -11, -15, -19, -25, -30,
                    -34, -31, -30, -31, -29, -32, -35, -42,
                    -48, -42, -44, -46, -50, -50, -51, -52,
                    -59, -54, -55, -55, -58, -62, -63, -66,
                    -72, -73, -76, -75, -78, -80, -80, -81,
                    -84, -88, -90, -94, -98, -101, -106, -110
                }
            },
            /* 88Hz */
            new[]
            {
                new float[]
                {
                    -66, -66, -66, -66, -66, -66, -66, -66,
                    -66, -66, -66, -66, -66, -67, -67, -67,
                    -76, -72, -71, -74, -76, -76, -75, -78,
                    -79, -79, -81, -83, -86, -89, -93, -97,
                    -100, -105, -110, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -47, -47, -47, -47, -47, -47, -47, -47,
                    -47, -47, -47, -48, -51, -55, -59, -66,
                    -66, -66, -67, -66, -68, -69, -70, -74,
                    -79, -77, -77, -78, -80, -81, -82, -84,
                    -86, -88, -91, -95, -100, -108, -116, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -36, -36, -36, -36, -36, -36, -36, -36,
                    -36, -37, -37, -41, -44, -48, -51, -58,
                    -62, -60, -57, -59, -59, -60, -63, -65,
                    -72, -71, -70, -72, -74, -77, -76, -78,
                    -81, -81, -80, -83, -86, -91, -96, -100,
                    -105, -110, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -28, -28, -28, -28, -28, -28, -28, -28,
                    -28, -30, -32, -32, -33, -35, -41, -49,
                    -50, -49, -47, -48, -48, -52, -51, -57,
                    -65, -61, -59, -61, -64, -69, -70, -74,
                    -77, -77, -78, -81, -84, -85, -87, -90,
                    -92, -96, -100, -107, -112, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -19, -19, -19, -19, -19, -19, -19, -19,
                    -20, -21, -23, -27, -30, -35, -36, -41,
                    -46, -44, -42, -40, -41, -41, -43, -48,
                    -55, -53, -52, -53, -56, -59, -58, -60,
                    -67, -66, -69, -71, -72, -75, -79, -81,
                    -84, -87, -90, -93, -97, -101, -107, -114,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -9, -9, -9, -9, -9, -9, -9, -9,
                    -11, -12, -12, -15, -16, -20, -23, -30,
                    -37, -34, -33, -34, -31, -32, -32, -38,
                    -47, -44, -41, -40, -47, -49, -46, -46,
                    -58, -50, -50, -54, -58, -62, -64, -67,
                    -67, -70, -72, -76, -79, -83, -87, -91,
                    -96, -100, -104, -110, -999, -999, -999, -999
                }
            },
            /* 125 Hz */
            new[]
            {
                new float[]
                {
                    -62, -62, -62, -62, -62, -62, -62, -62,
                    -62, -62, -63, -64, -66, -67, -66, -68,
                    -75, -72, -76, -75, -76, -78, -79, -82,
                    -84, -85, -90, -94, -101, -110, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -59, -59, -59, -59, -59, -59, -59, -59,
                    -59, -59, -59, -60, -60, -61, -63, -66,
                    -71, -68, -70, -70, -71, -72, -72, -75,
                    -81, -78, -79, -82, -83, -86, -90, -97,
                    -103, -113, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -53, -53, -53, -53, -53, -53, -53, -53,
                    -53, -54, -55, -57, -56, -57, -55, -61,
                    -65, -60, -60, -62, -63, -63, -66, -68,
                    -74, -73, -75, -75, -78, -80, -80, -82,
                    -85, -90, -96, -101, -108, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -46, -46, -46, -46, -46, -46, -46, -46,
                    -46, -46, -47, -47, -47, -47, -48, -51,
                    -57, -51, -49, -50, -51, -53, -54, -59,
                    -66, -60, -62, -67, -67, -70, -72, -75,
                    -76, -78, -81, -85, -88, -94, -97, -104,
                    -112, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -36, -36, -36, -36, -36, -36, -36, -36,
                    -39, -41, -42, -42, -39, -38, -41, -43,
                    -52, -44, -40, -39, -37, -37, -40, -47,
                    -54, -50, -48, -50, -55, -61, -59, -62,
                    -66, -66, -66, -69, -69, -73, -74, -74,
                    -75, -77, -79, -82, -87, -91, -95, -100,
                    -108, -115, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -28, -26, -24, -22, -20, -20, -23, -29,
                    -30, -31, -28, -27, -28, -28, -28, -35,
                    -40, -33, -32, -29, -30, -30, -30, -37,
                    -45, -41, -37, -38, -45, -47, -47, -48,
                    -53, -49, -48, -50, -49, -49, -51, -52,
                    -58, -56, -57, -56, -60, -61, -62, -70,
                    -72, -74, -78, -83, -88, -93, -100, -106
                }
            },
            /* 177 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -110, -105, -100, -95, -91, -87, -83,
                    -80, -78, -76, -78, -78, -81, -83, -85,
                    -86, -85, -86, -87, -90, -97, -107, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -110, -105, -100, -95, -90,
                    -85, -81, -77, -73, -70, -67, -67, -68,
                    -75, -73, -70, -69, -70, -72, -75, -79,
                    -84, -83, -84, -86, -88, -89, -89, -93,
                    -98, -105, -112, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -105, -100, -95, -90, -85, -80, -76, -71,
                    -68, -68, -65, -63, -63, -62, -62, -64,
                    -65, -64, -61, -62, -63, -64, -66, -68,
                    -73, -73, -74, -75, -76, -81, -83, -85,
                    -88, -89, -92, -95, -100, -108, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -80, -75, -71, -68, -65, -63, -62, -61,
                    -61, -61, -61, -59, -56, -57, -53, -50,
                    -58, -52, -50, -50, -52, -53, -54, -58,
                    -67, -63, -67, -68, -72, -75, -78, -80,
                    -81, -81, -82, -85, -89, -90, -93, -97,
                    -101, -107, -114, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -65, -61, -59, -57, -56, -55, -55, -56,
                    -56, -57, -55, -53, -52, -47, -44, -44,
                    -50, -44, -41, -39, -39, -42, -40, -46,
                    -51, -49, -50, -53, -54, -63, -60, -61,
                    -62, -66, -66, -66, -70, -73, -74, -75,
                    -76, -75, -79, -85, -89, -91, -96, -102,
                    -110, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -52, -50, -49, -49, -48, -48, -48, -49,
                    -50, -50, -49, -46, -43, -39, -35, -33,
                    -38, -36, -32, -29, -32, -32, -32, -35,
                    -44, -39, -38, -38, -46, -50, -45, -46,
                    -53, -50, -50, -50, -54, -54, -53, -53,
                    -56, -57, -59, -66, -70, -72, -74, -79,
                    -83, -85, -90, -97, -114, -999, -999, -999
                }
            },
            /* 250 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -110, -105,
                    -100, -95, -90, -86, -80, -75, -75, -79,
                    -80, -79, -80, -81, -82, -88, -95, -103,
                    -110, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -108, -103, -98, -93,
                    -88, -83, -79, -78, -75, -71, -67, -68,
                    -73, -73, -72, -73, -75, -77, -80, -82,
                    -88, -93, -100, -107, -114, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -110, -105, -101, -96, -90,
                    -86, -81, -77, -73, -69, -66, -61, -62,
                    -66, -64, -62, -65, -66, -70, -72, -76,
                    -81, -80, -84, -90, -95, -102, -110, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -107, -103, -97, -92, -88,
                    -83, -79, -74, -70, -66, -59, -53, -58,
                    -62, -55, -54, -54, -54, -58, -61, -62,
                    -72, -70, -72, -75, -78, -80, -81, -80,
                    -83, -83, -88, -93, -100, -107, -115, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -105, -100, -95, -90, -85,
                    -80, -75, -70, -66, -62, -56, -48, -44,
                    -48, -46, -46, -43, -46, -48, -48, -51,
                    -58, -58, -59, -60, -62, -62, -61, -61,
                    -65, -64, -65, -68, -70, -74, -75, -78,
                    -81, -86, -95, -110, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -105, -100, -95, -90, -85, -80,
                    -75, -70, -65, -61, -55, -49, -39, -33,
                    -40, -35, -32, -38, -40, -33, -35, -37,
                    -46, -41, -45, -44, -46, -42, -45, -46,
                    -52, -50, -50, -50, -54, -54, -55, -57,
                    -62, -64, -66, -68, -70, -76, -81, -90,
                    -100, -110, -999, -999, -999, -999, -999, -999
                }
            },
            /* 354 hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -105, -98, -90, -85, -82, -83, -80, -78,
                    -84, -79, -80, -83, -87, -89, -91, -93,
                    -99, -106, -117, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -105, -98, -90, -85, -80, -75, -70, -68,
                    -74, -72, -74, -77, -80, -82, -85, -87,
                    -92, -89, -91, -95, -100, -106, -112, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -105, -98, -90, -83, -75, -71, -63, -64,
                    -67, -62, -64, -67, -70, -73, -77, -81,
                    -84, -83, -85, -89, -90, -93, -98, -104,
                    -109, -114, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -103, -96, -88, -81, -75, -68, -58, -54,
                    -56, -54, -56, -56, -58, -60, -63, -66,
                    -74, -69, -72, -72, -75, -74, -77, -81,
                    -81, -82, -84, -87, -93, -96, -99, -104,
                    -110, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -108, -102, -96,
                    -91, -85, -80, -74, -68, -60, -51, -46,
                    -48, -46, -43, -45, -47, -47, -49, -48,
                    -56, -53, -55, -58, -57, -63, -58, -60,
                    -66, -64, -67, -70, -70, -74, -77, -84,
                    -86, -89, -91, -93, -94, -101, -109, -118,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -108, -103, -98, -93, -88,
                    -83, -78, -73, -68, -60, -53, -44, -35,
                    -38, -38, -34, -34, -36, -40, -41, -44,
                    -51, -45, -46, -47, -46, -54, -50, -49,
                    -50, -50, -50, -51, -54, -57, -58, -60,
                    -66, -66, -66, -64, -65, -68, -77, -82,
                    -87, -95, -110, -999, -999, -999, -999, -999
                }
            },
            /* 500 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -107, -102, -97, -92, -87, -83, -78, -75,
                    -82, -79, -83, -85, -89, -92, -95, -98,
                    -101, -105, -109, -113, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -106,
                    -100, -95, -90, -86, -81, -78, -74, -69,
                    -74, -74, -76, -79, -83, -84, -86, -89,
                    -92, -97, -93, -100, -103, -107, -110, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -106, -100,
                    -95, -90, -87, -83, -80, -75, -69, -60,
                    -66, -66, -68, -70, -74, -78, -79, -81,
                    -81, -83, -84, -87, -93, -96, -99, -103,
                    -107, -110, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -108, -103, -98,
                    -93, -89, -85, -82, -78, -71, -62, -55,
                    -58, -58, -54, -54, -55, -59, -61, -62,
                    -70, -66, -66, -67, -70, -72, -75, -78,
                    -84, -84, -84, -88, -91, -90, -95, -98,
                    -102, -103, -106, -110, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -108, -103, -98, -94,
                    -90, -87, -82, -79, -73, -67, -58, -47,
                    -50, -45, -41, -45, -48, -44, -44, -49,
                    -54, -51, -48, -47, -49, -50, -51, -57,
                    -58, -60, -63, -69, -70, -69, -71, -74,
                    -78, -82, -90, -95, -101, -105, -110, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -105, -101, -97, -93, -90,
                    -85, -80, -77, -72, -65, -56, -48, -37,
                    -40, -36, -34, -40, -50, -47, -38, -41,
                    -47, -38, -35, -39, -38, -43, -40, -45,
                    -50, -45, -44, -47, -50, -55, -48, -48,
                    -52, -66, -70, -76, -82, -90, -97, -105,
                    -110, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 707 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -108, -103, -98, -93, -86, -79, -76,
                    -83, -81, -85, -87, -89, -93, -98, -102,
                    -107, -112, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -108, -103, -98, -93, -86, -79, -71,
                    -77, -74, -77, -79, -81, -84, -85, -90,
                    -92, -93, -92, -98, -101, -108, -112, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -108, -103, -98, -93, -87, -78, -68, -65,
                    -66, -62, -65, -67, -70, -73, -75, -78,
                    -82, -82, -83, -84, -91, -93, -98, -102,
                    -106, -110, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -105, -100, -95, -90, -82, -74, -62, -57,
                    -58, -56, -51, -52, -52, -54, -54, -58,
                    -66, -59, -60, -63, -66, -69, -73, -79,
                    -83, -84, -80, -81, -81, -82, -88, -92,
                    -98, -105, -113, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -107,
                    -102, -97, -92, -84, -79, -69, -57, -47,
                    -52, -47, -44, -45, -50, -52, -42, -42,
                    -53, -43, -43, -48, -51, -56, -55, -52,
                    -57, -59, -61, -62, -67, -71, -78, -83,
                    -86, -94, -98, -103, -110, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -105, -100,
                    -95, -90, -84, -78, -70, -61, -51, -41,
                    -40, -38, -40, -46, -52, -51, -41, -40,
                    -46, -40, -38, -38, -41, -46, -41, -46,
                    -47, -43, -43, -45, -41, -45, -56, -67,
                    -68, -83, -87, -90, -95, -102, -107, -113,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 1000 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -109, -105, -101, -96, -91, -84, -77,
                    -82, -82, -85, -89, -94, -100, -106, -110,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -106, -103, -98, -92, -85, -80, -71,
                    -75, -72, -76, -80, -84, -86, -89, -93,
                    -100, -107, -113, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -107,
                    -104, -101, -97, -92, -88, -84, -80, -64,
                    -66, -63, -64, -66, -69, -73, -77, -83,
                    -83, -86, -91, -98, -104, -111, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -107,
                    -104, -101, -97, -92, -90, -84, -74, -57,
                    -58, -52, -55, -54, -50, -52, -50, -52,
                    -63, -62, -69, -76, -77, -78, -78, -79,
                    -82, -88, -94, -100, -106, -111, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -106, -102,
                    -98, -95, -90, -85, -83, -78, -70, -50,
                    -50, -41, -44, -49, -47, -50, -50, -44,
                    -55, -46, -47, -48, -48, -54, -49, -49,
                    -58, -62, -71, -81, -87, -92, -97, -102,
                    -108, -114, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -106, -102,
                    -98, -95, -90, -85, -83, -78, -70, -45,
                    -43, -41, -47, -50, -51, -50, -49, -45,
                    -47, -41, -44, -41, -39, -43, -38, -37,
                    -40, -41, -44, -50, -58, -65, -73, -79,
                    -85, -92, -97, -101, -105, -109, -113, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 1414 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -107, -100, -95, -87, -81,
                    -85, -83, -88, -93, -100, -107, -114, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -107, -101, -95, -88, -83, -76,
                    -73, -72, -79, -84, -90, -95, -100, -105,
                    -110, -115, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -104, -98, -92, -87, -81, -70,
                    -65, -62, -67, -71, -74, -80, -85, -91,
                    -95, -99, -103, -108, -111, -114, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -103, -97, -90, -85, -76, -60,
                    -56, -54, -60, -62, -61, -56, -63, -65,
                    -73, -74, -77, -75, -78, -81, -86, -87,
                    -88, -91, -94, -98, -103, -110, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -105,
                    -100, -97, -92, -86, -81, -79, -70, -57,
                    -51, -47, -51, -58, -60, -56, -53, -50,
                    -58, -52, -50, -50, -53, -55, -64, -69,
                    -71, -85, -82, -78, -81, -85, -95, -102,
                    -112, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -105,
                    -100, -97, -92, -85, -83, -79, -72, -49,
                    -40, -43, -43, -54, -56, -51, -50, -40,
                    -43, -38, -36, -35, -37, -38, -37, -44,
                    -54, -60, -57, -60, -70, -75, -84, -92,
                    -103, -112, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 2000 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -110, -102, -95, -89, -82,
                    -83, -84, -90, -92, -99, -107, -113, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -107, -101, -95, -89, -83, -72,
                    -74, -78, -85, -88, -88, -90, -92, -98,
                    -105, -111, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -109, -103, -97, -93, -87, -81, -70,
                    -70, -67, -75, -73, -76, -79, -81, -83,
                    -88, -89, -97, -103, -110, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -107, -100, -94, -88, -83, -75, -63,
                    -59, -59, -63, -66, -60, -62, -67, -67,
                    -77, -76, -81, -88, -86, -92, -96, -102,
                    -109, -116, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -105, -98, -92, -86, -81, -73, -56,
                    -52, -47, -55, -60, -58, -52, -51, -45,
                    -49, -50, -53, -54, -61, -71, -70, -69,
                    -78, -79, -87, -90, -96, -104, -112, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -103, -96, -90, -86, -78, -70, -51,
                    -42, -47, -48, -55, -54, -54, -53, -42,
                    -35, -28, -33, -38, -37, -44, -47, -49,
                    -54, -63, -68, -78, -82, -89, -94, -99,
                    -104, -109, -114, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 2828 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -110, -100, -90, -79,
                    -85, -81, -82, -82, -89, -94, -99, -103,
                    -109, -115, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -105, -97, -85, -72,
                    -74, -70, -70, -70, -76, -85, -91, -93,
                    -97, -103, -109, -115, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -112, -93, -81, -68,
                    -62, -60, -60, -57, -63, -70, -77, -82,
                    -90, -93, -98, -104, -109, -113, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -113, -100, -93, -84, -63,
                    -58, -48, -53, -54, -52, -52, -57, -64,
                    -66, -76, -83, -81, -85, -85, -90, -95,
                    -98, -101, -103, -106, -108, -111, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -105, -95, -86, -74, -53,
                    -50, -38, -43, -49, -43, -42, -39, -39,
                    -46, -52, -57, -56, -72, -69, -74, -81,
                    -87, -92, -94, -97, -99, -102, -105, -108,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -108, -99, -90, -76, -66, -45,
                    -43, -41, -44, -47, -43, -47, -40, -30,
                    -31, -31, -39, -33, -40, -41, -43, -53,
                    -59, -70, -73, -77, -79, -82, -84, -87,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 4000 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -110, -91, -76,
                    -75, -85, -93, -98, -104, -110, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -110, -91, -70,
                    -70, -75, -86, -89, -94, -98, -101, -106,
                    -110, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -110, -95, -80, -60,
                    -65, -64, -74, -83, -88, -91, -95, -99,
                    -103, -107, -110, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -110, -95, -80, -58,
                    -55, -49, -66, -68, -71, -78, -78, -80,
                    -88, -85, -89, -97, -100, -105, -110, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -110, -95, -80, -53,
                    -52, -41, -59, -59, -49, -58, -56, -63,
                    -86, -79, -90, -93, -98, -103, -107, -112,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -110, -97, -91, -73, -45,
                    -40, -33, -53, -61, -49, -54, -50, -50,
                    -60, -52, -67, -74, -81, -92, -96, -100,
                    -105, -110, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 5657 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -113, -106, -99, -92, -77,
                    -80, -88, -97, -106, -115, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -116, -109, -102, -95, -89, -74,
                    -72, -88, -87, -95, -102, -109, -116, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -116, -109, -102, -95, -89, -75,
                    -66, -74, -77, -78, -86, -87, -90, -96,
                    -105, -115, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -115, -108, -101, -94, -88, -66,
                    -56, -61, -70, -65, -78, -72, -83, -84,
                    -93, -98, -105, -110, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -110, -105, -95, -89, -82, -57,
                    -52, -52, -59, -56, -59, -58, -69, -67,
                    -88, -82, -82, -89, -94, -100, -108, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -110, -101, -96, -90, -83, -77, -54,
                    -43, -38, -50, -48, -52, -48, -42, -42,
                    -51, -52, -53, -59, -65, -71, -78, -85,
                    -95, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 8000 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -120, -105, -86, -68,
                    -78, -79, -90, -100, -110, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -120, -105, -86, -66,
                    -73, -77, -88, -96, -105, -115, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -120, -105, -92, -80, -61,
                    -64, -68, -80, -87, -92, -100, -110, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -120, -104, -91, -79, -52,
                    -60, -54, -64, -69, -77, -80, -82, -84,
                    -85, -87, -88, -90, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -118, -100, -87, -77, -49,
                    -50, -44, -58, -61, -61, -67, -65, -62,
                    -62, -62, -65, -68, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -115, -98, -84, -62, -49,
                    -44, -38, -46, -49, -49, -46, -39, -37,
                    -39, -40, -42, -43, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 11314 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -110, -88, -74,
                    -77, -82, -82, -85, -90, -94, -99, -104,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -110, -88, -66,
                    -70, -81, -80, -81, -84, -88, -91, -93,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -110, -88, -61,
                    -63, -70, -71, -74, -77, -80, -83, -85,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -110, -86, -62,
                    -63, -62, -62, -58, -52, -50, -50, -52,
                    -54, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -118, -108, -84, -53,
                    -50, -50, -50, -55, -47, -45, -40, -40,
                    -40, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -118, -100, -73, -43,
                    -37, -42, -43, -53, -38, -37, -35, -35,
                    -38, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            },
            /* 16000 Hz */
            new[]
            {
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -110, -100, -91, -84, -74,
                    -80, -80, -80, -80, -80, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -110, -100, -91, -84, -74,
                    -68, -68, -68, -68, -68, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -110, -100, -86, -78, -70,
                    -60, -45, -30, -21, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -110, -100, -87, -78, -67,
                    -48, -38, -29, -21, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -110, -100, -86, -69, -56,
                    -45, -35, -33, -29, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                },
                new float[]
                {
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -110, -100, -83, -71, -48,
                    -27, -38, -37, -34, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999,
                    -999, -999, -999, -999, -999, -999, -999, -999
                }
            }
        };

        #endregion
    }
}