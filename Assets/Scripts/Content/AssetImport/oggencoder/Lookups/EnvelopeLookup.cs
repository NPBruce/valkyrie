using System;
using System.Collections.Generic;
using System.Linq;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{
    public class EnvelopeLookup
    {
        public const int EnvelopePost = 2;
        private const int SearchStep = 64;
        private const int WindowLength = 128;
        private const int EnvelopeWindow = 4;
        private const int EnvelopeMinStretch = 2;
        private const int EnvelopeMaxStretch = 12; // One third full block
        private readonly EnvelopeBand[] _bands;
        private readonly List<EnvelopeFilterState> _filters;
        private readonly MdctLookup _mdctLookup;

        private readonly float[] _mdctWindow;
        private readonly float _minEnergy;
        private readonly PsyGlobal _psyGlobal;
        private int _current;
        private int _currentMark;
        private int _cursor;

        private int[] _mark;
        private int _stretch;

        public EnvelopeLookup(PsyGlobal psyGlobal, VorbisInfo info)
        {
            _psyGlobal = psyGlobal;
            var codecSetup = info.CodecSetup;

            _minEnergy = codecSetup.PsyGlobalParam.PreEchoMinEnergy;
            _cursor = codecSetup.BlockSizes[1]/2;
            _mdctWindow = new float[WindowLength];
            _mdctLookup = new MdctLookup(WindowLength);

            for (var i = 0; i < _mdctWindow.Length; i++)
            {
                _mdctWindow[i] = (float) Math.Sin(i/(WindowLength - 1.0)*Math.PI);
                _mdctWindow[i] *= _mdctWindow[i];
            }

            _bands = new EnvelopeBand[PsyGlobal.EnvelopeBands];

            // Magic follows
            _bands[0] = new EnvelopeBand(2, 4);
            _bands[1] = new EnvelopeBand(4, 5);
            _bands[2] = new EnvelopeBand(6, 6);
            _bands[3] = new EnvelopeBand(9, 8);
            _bands[4] = new EnvelopeBand(13, 8);
            _bands[5] = new EnvelopeBand(17, 8);
            _bands[6] = new EnvelopeBand(22, 8);

            _filters = new List<EnvelopeFilterState>(Enumerable
                .Range(0, PsyGlobal.EnvelopeBands*info.Channels)
                .Select(s => new EnvelopeFilterState())
                .ToArray());

            _mark = new int[WindowLength];
        }

        public void Shift(int shift)
        {
            var smallsize = _current/SearchStep + EnvelopePost;
            var smallshift = shift/SearchStep;

            Array.Copy(_mark, smallshift, _mark, 0, smallsize - smallshift);

            _current -= shift;
            if (_currentMark >= 0)
                _currentMark -= shift;
            _cursor -= shift;
        }

        public bool Mark(int beginWindow, int endWindow)
        {
            if ((_currentMark >= beginWindow) && (_currentMark < endWindow))
                return true;

            var first = beginWindow/SearchStep;
            var last = endWindow/SearchStep;

            for (var i = first; i < last; i++)
                if (_mark[i] != 0)
                    return true;

            return false;
        }

        public int Search(
            float[][] pcm,
            int pcmCurrent,
            int centerWindow,
            int testWindow)
        {
            var first = _current/SearchStep;
            var last = pcmCurrent/SearchStep - EnvelopeWindow;

            if (first < 0)
                first = 0;

            // make sure we have enough storage to match the PCM
            var requiredStorage = last + EnvelopeWindow + EnvelopePost;
            if (requiredStorage > _mark.Length)
                _mark = new int[requiredStorage];

            for (var j = first; j < last; j++)
            {
                var ret = 0;

                _stretch++;
                if (_stretch > EnvelopeMaxStretch*2)
                    _stretch = EnvelopeMaxStretch*2;

                for (var channel = 0; channel < pcm.Length; channel++)
                    ret |= AmpPcm(
                        new List<float>(pcm[channel]),
                        SearchStep*j,
                        channel*PsyGlobal.EnvelopeBands);

                _mark[j + EnvelopePost] = 0;
                if ((ret & 1) != 0)
                {
                    _mark[j] = 1;
                    _mark[j + 1] = 1;
                }

                if ((ret & 2) != 0)
                {
                    _mark[j] = 1;
                    if (j > 0)
                        _mark[j - 1] = 1;
                }

                if ((ret & 4) != 0)
                    _stretch = -1;
            }

            _current = last*SearchStep;

            var l = _cursor;

            while (l < _current - SearchStep)
            {
                // account for postecho working back one window 
                if (l >= testWindow)
                    return 1;

                _cursor = l;

                if (_mark[l/SearchStep] != 0)
                    if (l > centerWindow)
                    {
                        _currentMark = l;
                        return l >= testWindow ? 1 : 0;
                    }

                l += SearchStep;
            }

            return -1;
        }

        private int AmpPcm(
            List<float> pcm,
            int pcmOffset,
            int filterOffset)
        {
            var ret = 0;

            // we want to have a 'minimum bar' for energy, else we're just
            // basing blocks on quantization noise that outweighs the signal
            // itself (for low power signals) 
            var vec = new float[WindowLength];

            // stretch is used to gradually lengthen the number of windows considered previous-to-potential-trigger 
            var penalty = _psyGlobal.StretchPenalty - (_stretch/2 - EnvelopeMinStretch);

            if (penalty < 0f)
                penalty = 0f;

            if (penalty > _psyGlobal.StretchPenalty)
                penalty = _psyGlobal.StretchPenalty;

            // window and transform 
            for (var i = 0; i < vec.Length; i++)
                vec[i] = pcm[pcmOffset + i]*_mdctWindow[i];

            _mdctLookup.Forward(vec, vec);

            // near-DC spreading function; ampPtr has nothing to do with
            // psychoacoustics, just sidelobe leakage and window size 
            var temp = (float) (vec[0]*vec[0] + .7*vec[1]*vec[1] + .2*vec[2]*vec[2]);
            var decay = _filters[filterOffset].SpreadNearDc(temp);

            // perform spreading and limiting, also smooth the spectrum.  yes,
            // the MDCT results in all real coefficients, but it still *behaves*
            // like real/imaginary pairs 
            for (var i = 0; i < WindowLength/2; i += 2)
            {
                var val = vec[i]*vec[i] + vec[i + 1]*vec[i + 1];
                val = val.ToDecibel()*.5f;

                if (val < decay)
                    val = decay;

                if (val < _minEnergy)
                    val = _minEnergy;

                vec[i >> 1] = val;
                decay -= 8;
            }

            // perform preecho/postecho triggering by band 
            for (var j = 0; j < _bands.Length; j++)
            {
                // accumulate amplitude 
                float acc = 0;
                for (var i = 0; i < _bands[j].Window.Length; i++)
                    acc += vec[i + _bands[j].Begin]*_bands[j].Window[i];

                acc *= _bands[j].Total;

                // convert amplitude to delta 
                var stretch = Math.Max(EnvelopeMinStretch, _stretch/2);
                var delta = _filters[filterOffset + j].ConvertAmplitudeToDelta(acc, stretch);

                // look at min/max, decide trigger 
                if (delta.Max > _psyGlobal.PreEchoThreshold[j] + penalty)
                {
                    ret |= 1;
                    ret |= 4;
                }

                if (delta.Min < _psyGlobal.PostEchoThreshold[j] - penalty)
                    ret |= 2;
            }

            return ret;
        }
    }
}