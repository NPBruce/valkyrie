using System;
using System.Collections.Generic;
using OggVorbisEncoder.Lookups;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{
    /// <summary>
    ///     Buffers the current vorbis audio analysis/synthesis state.
    ///     The DSP state belongs to a specific logical bitstream
    /// </summary>
    public class ProcessingState
    {
        // .345 is a hack; the original ToDecibel estimation used on IEEE 754
        // compliant machines had an error that returned dB values about a third
        // of a decibel too high.  The error was harmless because tunings
        // implicitly took that into account.  However, fixing the error
        // in the estimator requires changing all the tunings as well.
        // For now, it's easier to sync things back up here, and
        // recalibrate the tunings in the next major model upgrade. 
        private const float DecibelOffset = .345f;
        private readonly LookupCollection _lookups;
        private readonly float[][] _pcm;

        private readonly VorbisInfo _vorbisInfo;
        private readonly List<int> _window;

        private int _centerWindow;
        private int _currentWindow;
        private int _eofFlag;
        private int _granulePosition;
        private int _lastWindow;
        private int _nextWindow;
        private int _pcmCurrent;
        private bool _preExtrapolated;
        private int _sequence = 3; // compressed audio packets start after the headers with sequence number 3 

        private ProcessingState(
            VorbisInfo vorbisInfo,
            LookupCollection lookups,
            float[][] pcm,
            List<int> window,
            int centerWindow)
        {
            _vorbisInfo = vorbisInfo;
            _lookups = lookups;
            _pcm = pcm;
            _window = window;
            _centerWindow = centerWindow;
        }

        /// <summary>
        ///     Writes the provided data to the pcm buffer
        /// </summary>
        /// <param name="data">The data to write in an array of dimensions: channels * length</param>
        /// <param name="length">The number of elements to write</param>
        public void WriteData(float[][] data, int length)
        {
            if (length <= 0)
                return;

            EnsureBufferSize(length);

            for (var i = 0; i < data.Length; ++i)
                Array.Copy(data[i], 0, _pcm[i], _pcmCurrent, length);

            var vi = _vorbisInfo;
            var ci = vi.CodecSetup;
            _pcmCurrent += length;

            // we may want to reverse extrapolate the beginning of a stream too... in case we're beginning on a cliff! 
            // clumsy, but simple.  It only runs once, so simple is good. 
            if (!_preExtrapolated && (_pcmCurrent - _centerWindow > ci.BlockSizes[1]))
                PreExtrapolate();
        }

        public void WriteEndOfStream()
        {
            var ci = _vorbisInfo.CodecSetup;

            const int order = 32;
            var lpc = new float[order];

            // if it wasn't done earlier (very short sample) 
            if (!_preExtrapolated)
                PreExtrapolate();

            // We're encoding the end of the stream.  Just make sure we have [at least] a few full blocks of zeroes at the end. 
            // actually, we don't want zeroes; that could drop a large amplitude off a cliff, creating spread spectrum noise that will
            // suck to encode.  Extrapolate for the sake of cleanliness.
            EnsureBufferSize(ci.BlockSizes[1]*3);

            _eofFlag = _pcmCurrent;
            _pcmCurrent += ci.BlockSizes[1]*3;

            for (var channel = 0; channel < _pcm.Length; channel++)
                if (_eofFlag > order*2)
                {
                    // extrapolate with LPC to fill in
                    // make and run a predictor filter 
                    var n = _eofFlag;
                    if (n > ci.BlockSizes[1])
                        n = ci.BlockSizes[1];

                    PopulateLpcFromPcm(new List<float>(lpc), _pcm[channel], _eofFlag - n, n, order);
                    UpdatePcmFromLpcPredict(new List<float>(lpc), _pcm[channel], _eofFlag, order, _pcmCurrent - _eofFlag);
                }
                else
                {
                    // not enough data to extrapolate (unlikely to happen due to guarding the overlap, 
                    // but bulletproof in case that assumption goes away). zeroes will do.
                    Array.Clear(_pcm[channel], _eofFlag, _pcmCurrent - _eofFlag);
                }
        }

        private static void UpdatePcmFromLpcPredict(
            List<float> lpcCoeff,
            IList<float> data,
            int offset,
            int m,
            int n)
        {
            var work = new float[m + n];

            for (var i = 0; i < m; i++)
                work[i] = data[offset - m + i];

            for (var i = 0; i < n; i++)
            {
                int o = i, p = m;
                float y = 0;

                for (var j = 0; j < m; j++)
                    y = y - work[o++]*lpcCoeff[--p];

                data[offset + i] = work[o] = y;
            }
        }

        private static void PopulateLpcFromPcm(IList<float> lpci, IList<float> data, int offset, int n, int m)
        {
            var aut = new double[m + 1];
            var lpc = new double[m];

            // autocorrelation, p+1 lag coefficients 
            var j = m + 1;
            while (j-- > 0)
            {
                double d = 0; // double needed for accumulator depth 
                for (var i = j; i < n; i++)
                    d += (double) data[offset + i]
                         *data[offset + i - j];

                aut[j] = d;
            }

            // Generate lpc coefficients from autocorr values 
            // set our noise floor to about -100dB 
            var error = aut[0]*(1.0 + 1e-10);
            var epsilon = 1e-9*aut[0] + 1e-10;

            for (var i = 0; i < m; i++)
            {
                var r = -aut[i + 1];

                if (error < epsilon)
                {
                    Array.Clear(lpc, i, m - i);
                    break;
                }

                // Sum up ampPtr iteration's reflection coefficient; note that in
                // Vorbis we don't save it.  If anyone wants to recycle ampPtr code
                // and needs reflection coefficients, save the results of 'r' from
                // each iteration. 
                for (j = 0; j < i; j++)
                    r -= lpc[j]*aut[i - j];

                r /= error;

                // Update LPC coefficients and total error 
                lpc[i] = r;
                for (j = 0; j < i/2; j++)
                {
                    var tmp = lpc[j];
                    lpc[j] += r*lpc[i - 1 - j];
                    lpc[i - 1 - j] += r*tmp;
                }

                if ((i & 1) != 0)
                    lpc[j] += lpc[j]*r;

                error *= 1.0 - r*r;
            }

            // slightly damp the filter 
            {
                var g = .99;
                var damp = g;
                for (j = 0; j < m; j++)
                {
                    lpc[j] *= damp;
                    damp *= g;
                }
            }

            for (j = 0; j < m; j++)
                lpci[j] = (float) lpc[j];
        }

        private void PreExtrapolate()
        {
            const int order = 16;

            _preExtrapolated = true;

            if (_pcmCurrent - _centerWindow <= order*2)
                return;

            var lpc = new float[order];
            var work = new float[_pcmCurrent];

            for (var channel = 0; channel < _pcm.Length; channel++)
            {
                // need to run the extrapolation in reverse! 
                for (var j = 0; j < _pcmCurrent; j++)
                    work[j] = _pcm[channel][_pcmCurrent - j - 1];

                // prime as above 
                PopulateLpcFromPcm(new List<float>(lpc), work, 0, _pcmCurrent - _centerWindow, order);

                // run the predictor filter 
                UpdatePcmFromLpcPredict(new List<float>(lpc), work, _pcmCurrent - _centerWindow, order, _centerWindow);

                for (var j = 0; j < _pcmCurrent; j++)
                    _pcm[channel][_pcmCurrent - j - 1] = work[j];
            }
        }

        public void EnsureBufferSize(int needed)
        {
            var pcmStorage = _pcm[0].Length;
            if (_pcmCurrent + needed < pcmStorage)
                return;

            pcmStorage = _pcmCurrent + needed*2;

            for (var i = 0; i < _pcm.Length; i++)
            {
                var buffer = _pcm[i];
                Array.Resize(ref buffer, pcmStorage);
                _pcm[i] = buffer;
            }
        }

        public bool PacketOut(out OggPacket packet)
        {
            packet = null;

            // Have we started?
            if (!_preExtrapolated)
                return false;

            // Are we done?
            if (_eofFlag == -1)
                return false;

            var codecSetup = _vorbisInfo.CodecSetup;

            // By our invariant, we have lW, W and centerW set.  Search for
            // the next boundary so we can determine nW (the next window size)
            // which lets us compute the shape of the current block's window
            // we do an envelope search even on a single blocksize; we may still
            // be throwing more bits at impulses, and envelope search handles
            // marking impulses too. 
            var testWindow =
                _centerWindow +
                codecSetup.BlockSizes[_currentWindow]/4 +
                codecSetup.BlockSizes[1]/2 +
                codecSetup.BlockSizes[0]/4;

            var bp = _lookups.EnvelopeLookup.Search(_pcm, _pcmCurrent, _centerWindow, testWindow);

            if (bp == -1)
            {
                if (_eofFlag == 0)
                    return false; // not enough data currently to search for a full int block

                _nextWindow = 0;
            }
            else
            {
                _nextWindow = codecSetup.BlockSizes[0] == codecSetup.BlockSizes[1] ? 0 : bp;
            }

            var centerNext = _centerWindow
                             + codecSetup.BlockSizes[_currentWindow]/4
                             + codecSetup.BlockSizes[_nextWindow]/4;

            // center of next block + next block maximum right side.
            var blockbound = centerNext + codecSetup.BlockSizes[_nextWindow]/2;

            // Not enough data yet
            if (_pcmCurrent < blockbound)
                return false;

            // copy the vectors; ampPtr uses the local storage in vb 
            // ampPtr tracks 'strongest peak' for later psychoacoustics
            var n = codecSetup.BlockSizes[_currentWindow]/2;
            _lookups.PsyGlobalLookup.DecayAmpMax(n, _vorbisInfo.SampleRate);

            var pcmEnd = codecSetup.BlockSizes[_currentWindow];
            var pcm = new float[_pcm.Length][];
            var beginWindow = _centerWindow - codecSetup.BlockSizes[_currentWindow]/2;

            for (var channel = 0; channel < _pcm.Length; channel++)
            {
                pcm[channel] = new float[pcmEnd];
                Array.Copy(_pcm[channel], beginWindow, pcm[channel], 0, pcm[channel].Length);
            }

            // handle eof detection: eof==0 means that we've not yet received EOF eof>0  
            // marks the last 'real' sample in pcm[] eof<0  'no more to do'; doesn't get here 
            var eofFlag = false;
            if (_eofFlag != 0)
                if (_centerWindow >= _eofFlag)
                {
                    _eofFlag = -1;
                    eofFlag = true;
                }

            var data = PerformAnalysis(pcm, pcmEnd);
            packet = new OggPacket(data, eofFlag, _granulePosition, _sequence++);

            if (!eofFlag)
                AdvanceStorageVectors(centerNext);

            return true;
        }

        private void AdvanceStorageVectors(int centerNext)
        {
            // advance storage vectors and clean up 
            var newCenterNext = _vorbisInfo.CodecSetup.BlockSizes[1]/2;
            var movementW = centerNext - newCenterNext;

            if (movementW <= 0)
                return;

            _lookups.EnvelopeLookup.Shift(movementW);
            _pcmCurrent -= movementW;

            for (var channel = 0; channel < _pcm.Length; channel++)
                Array.Copy(_pcm[channel], movementW, _pcm[channel], 0, _pcmCurrent);

            _lastWindow = _currentWindow;
            _currentWindow = _nextWindow;
            _centerWindow = newCenterNext;

            if (_eofFlag != 0)
            {
                _eofFlag -= movementW;
                if (_eofFlag <= 0)
                    _eofFlag = -1;

                // do not add padding to end of stream! 
                if (_centerWindow >= _eofFlag)
                    _granulePosition += movementW - (_centerWindow - _eofFlag);
                else
                    _granulePosition += movementW;
            }
            else
            {
                _granulePosition += movementW;
            }
        }

        private bool MarkEnvelope()
        {
            var ve = _lookups.EnvelopeLookup;
            var ci = _vorbisInfo.CodecSetup;

            var beginW = _centerWindow - ci.BlockSizes[_currentWindow]/4;
            var endW = _centerWindow + ci.BlockSizes[_currentWindow]/4;

            if (_currentWindow != 0)
            {
                beginW -= ci.BlockSizes[_lastWindow]/4;
                endW += ci.BlockSizes[_nextWindow]/4;
            }
            else
            {
                beginW -= ci.BlockSizes[0]/4;
                endW += ci.BlockSizes[0]/4;
            }

            return ve.Mark(beginW, endW);
        }

        public static ProcessingState Create(VorbisInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var codecSetup = info.CodecSetup;

            // initialize the storage vectors. blocksize[1] is small for encode, but the correct size for decode 
            var pcmStorage = codecSetup.BlockSizes[1];

            var pcm = new float[info.Channels][];
            for (var i = 0; i < pcm.Length; i++)
                pcm[i] = new float[pcmStorage];

            // Vorbis I uses only window type 0
            var window = new int[2];
            window[0] = Encoding.Log(codecSetup.BlockSizes[0]) - 7;
            window[1] = Encoding.Log(codecSetup.BlockSizes[1]) - 7;

            var centerWindow = codecSetup.BlockSizes[1]/2;

            var lookups = LookupCollection.Create(info);

            return new ProcessingState(
                info,
                lookups,
                pcm,
                new List<int>(window),
                centerWindow);
        }

        private byte[] PerformAnalysis(
            float[][] pcm,
            int pcmEnd)
        {
            var channels = pcm.Length;

            var gmdct = new float[channels][];
            var work = new int[channels][];
            var floorPosts = new int[channels][][];
            var localAmpMax = new float[channels];

            var blockType = 0;
            if (_currentWindow != 0)
            {
                if ((_lastWindow != 0) && (_nextWindow != 0))
                    blockType = 1;
            }
            else
            {
                if (!MarkEnvelope())
                    blockType = 1;
            }

            var mapping = _vorbisInfo.CodecSetup.MapParams[_currentWindow];
            var psyLookup = _lookups.PsyLookup[blockType + (_currentWindow != 0 ? 2 : 0)];

            var buffer = new EncodeBuffer();

            TransformPcm(pcm, pcmEnd, work, gmdct, localAmpMax);
            ApplyMasks(pcm, pcmEnd, mapping, floorPosts, new List<float[]>(gmdct), psyLookup, new List<float>(localAmpMax));
            Encode(pcm, pcmEnd, buffer, mapping, new List<int[]>(work), new List<int[][]>(floorPosts), psyLookup, new List<float[]>(gmdct));

            return buffer.GetBytes();
        }

        private void Encode(
            float[][] pcm,
            int pcmEnd,
            EncodeBuffer buffer,
            Mapping mapping,
            List<int[]> work,
            List<int[][]> floorPosts,
            PsyLookup psyLookup,
            List<float[]> gmdct)
        {
            var codecSetup = _vorbisInfo.CodecSetup;
            var channels = pcm.Length;

            var nonzero = new bool[channels];

            //the next phases are performed once for vbr-only and PACKETBLOB
            //times for bitrate managed modes.

            //1) encode actual mode being used
            //2) encode the floor for each channel, compute coded mask curve/res
            //3) normalize and couple.
            //4) encode residue
            //5) save packet bytes to the packetblob vector

            // iterate over the many masking curve fits we've created 
            var coupleBundle = new int[channels][];
            var zerobundle = new bool[channels];

            const int k = PsyGlobal.PacketBlobs/2;

            // start out our new packet blob with packet type and mode 
            // Encode the packet type 
            buffer.Write(0, 1);
            // Encode the modenumber 
            // Encode frame mode, pre,post windowsize, then dispatch 
            var modeBits = Encoding.Log(codecSetup.ModeParams.Count - 1);
            buffer.Write((uint) _currentWindow, modeBits);
            if (_currentWindow != 0)
            {
                buffer.Write((uint) _lastWindow, 1);
                buffer.Write((uint) _nextWindow, 1);
            }

            // encode floor, compute masking curve, sep out residue 
            for (var i = 0; i < channels; i++)
            {
                var submap = mapping.ChannelMuxList[i];

                nonzero[i] = _lookups.FloorLookup[mapping.FloorSubMap[submap]].Encode(
                    buffer,
                    codecSetup.BookParams,
                    codecSetup.FullBooks,
                    floorPosts[i][k],
                    work[i],
                    pcmEnd,
                    codecSetup.BlockSizes[_currentWindow]/2);
            }

            // our iteration is now based on masking curve, not prequant and
            // coupling.  Only one prequant/coupling step quantize/couple 
            // incomplete implementation that assumes the tree is all depth
            // one, or no tree at all 
            psyLookup.CoupleQuantizeNormalize(
                k,
                codecSetup.PsyGlobalParam,
                mapping,
                gmdct,
                work,
                nonzero,
                codecSetup.PsyGlobalParam.SlidingLowPass[_currentWindow][k],
                channels);

            // classify and encode by submap 
            for (var i = 0; i < mapping.SubMaps; i++)
            {
                var channelsInBundle = 0;

                var resNumber = mapping.ResidueSubMap[i];

                for (var j = 0; j < channels; j++)
                    if (mapping.ChannelMuxList[j] == i)
                    {
                        zerobundle[channelsInBundle] = nonzero[j];
                        coupleBundle[channelsInBundle++] = work[j];
                    }

                var residue = _lookups.ResidueLookup[resNumber];
                var classifications = residue.Class(
                    coupleBundle,
                    zerobundle,
                    channelsInBundle);

                channelsInBundle = 0;
                for (var j = 0; j < channels; j++)
                    if (mapping.ChannelMuxList[j] == i)
                        coupleBundle[channelsInBundle++] = work[j];

                residue.Forward(
                    buffer,
                    pcmEnd,
                    coupleBundle,
                    zerobundle,
                    channelsInBundle,
                    classifications);
            }
        }

        private void TransformPcm(
            float[][] inputPcm,
            int pcmEnd,
            IList<int[]> work,
            IList<float[]> gmdct,
            IList<float> localAmpMax)
        {
            for (var channel = 0; channel < inputPcm.Length; channel++)
            {
                work[channel] = new int[pcmEnd/2];
                gmdct[channel] = new float[pcmEnd/2];

                var scale = 4f/pcmEnd;
                var scaleDecibel = scale.ToDecibel() + DecibelOffset;
                var pcm = inputPcm[channel];

                // window the PCM data 
                ApplyWindow(
                    pcm,
                    _lastWindow,
                    _currentWindow,
                    _nextWindow);

                // transform the PCM data - only MDCT right now..
                var transform = _lookups.TransformLookup[_currentWindow];
                transform.Forward(pcm, gmdct[channel]);

                // FFT yields more accurate tonal estimation (not phase sensitive) 
                var ffft = _lookups.FftLookup[_currentWindow];
                ffft.Forward(pcm);

                pcm[0] = scaleDecibel
                         + pcm[0].ToDecibel()
                         + DecibelOffset;

                localAmpMax[channel] = inputPcm[channel][0];

                for (var j = 1; j < pcmEnd - 1; j += 2)
                {
                    var temp = pcm[j]*pcm[j]
                               + pcm[j + 1]*pcm[j + 1];

                    var index = (j + 1) >> 1;
                    temp = pcm[index] = scaleDecibel + .5f
                                        *temp.ToDecibel()
                                        + DecibelOffset;

                    if (temp > localAmpMax[channel])
                        localAmpMax[channel] = temp;
                }

                if (localAmpMax[channel] > 0f)
                    localAmpMax[channel] = 0f;
            }
        }

        private void ApplyWindow(
            IList<float> pcm,
            int lastWindow,
            int window,
            int nextWindow)
        {
            lastWindow = window != 0 ? lastWindow : 0;
            nextWindow = window != 0 ? nextWindow : 0;

            var windowLastWindow = Block.Windows[_window[lastWindow]];
            var windowNextWindow = Block.Windows[_window[nextWindow]];

            var blockSizes = _vorbisInfo.CodecSetup.BlockSizes;
            var n = blockSizes[window];
            var ln = blockSizes[lastWindow];
            var rn = blockSizes[nextWindow];

            var leftbegin = n/4 - ln/4;
            var leftend = leftbegin + ln/2;

            var rightbegin = n/2 + n/4 - rn/4;
            var rightend = rightbegin + rn/2;

            int i, p;
            for (i = 0; i < leftbegin; i++)
                pcm[i] = 0f;

            for (p = 0; i < leftend; i++, p++)
                pcm[i] *= windowLastWindow[p];

            for (i = rightbegin, p = rn/2 - 1; i < rightend; i++, p--)
                pcm[i] *= windowNextWindow[p];

            for (; i < n; i++)
                pcm[i] = 0f;
        }

        private void ApplyMasks(
            float[][] inputPcm,
            int pcmEnd,
            Mapping mapping,
            IList<int[][]> floorPosts,
            List<float[]> gmdct,
            PsyLookup psyLookup,
            List<float> localAmpMax)
        {
            var noise = new float[pcmEnd/2];
            var tone = new float[pcmEnd/2];

            for (var channel = 0; channel < inputPcm.Length; channel++)
            {
                // the encoder setup assumes that all the modes used by any
                // specific bitrate tweaking use the same floor 
                var submap = mapping.ChannelMuxList[channel];

                var pcm = inputPcm[channel];
                var logmdct = new OffsetArray<float>(pcm, pcmEnd/2);

                floorPosts[channel] = new int[PsyGlobal.PacketBlobs][];

                for (var j = 0; j < pcmEnd/2; j++)
                    logmdct[j] = gmdct[channel][j].ToDecibel() + DecibelOffset;

                // first step; noise masking.  Not only does 'noise masking'
                // give us curves from which we can decide how much resolution
                // to give noise parts of the spectrum, it also implicitly hands
                // us a tonality estimate (the larger the value in the
                // 'noise_depth' vector, the more tonal that area is) 
                psyLookup.NoiseMask(logmdct, noise);

                var globalAmpMax = (int) _lookups.PsyGlobalLookup.AmpMax;
                foreach (var ampMax in localAmpMax)
                    if (ampMax > globalAmpMax)
                        globalAmpMax = (int) ampMax;

                // second step: 'all the other stuff'; all the stuff that isn't
                // computed/fit for bitrate management goes in the second psy
                // vector.  This includes tone masking, peak limiting and ATH 
                psyLookup.ToneMask(
                    pcm,
                    tone,
                    globalAmpMax,
                    localAmpMax[channel]);

                // third step; we offset the noise vectors, overlay tone
                //masking.  We then do a floor1-specific line fit.  If we're
                //performing bitrate management, the line fit is performed
                //multiple times for up/down tweakage on demand. 
                psyLookup.OffsetAndMix(
                    new List<float>(noise),
                    new List<float>(tone),
                    1,
                    pcm,
                    gmdct[channel],
                    logmdct);

                var floor = _lookups.FloorLookup[mapping.FloorSubMap[submap]];
                floorPosts[channel][PsyGlobal.PacketBlobs/2] = floor.Fit(logmdct, pcm);
            }
        }
    }
}