using System;

namespace OggVorbisEncoder.Lookups
{
    public class EnvelopeFilterState
    {
        private const int EnvelopePre = 16;
        private const int EnvelopeAmp = EnvelopePre + EnvelopeLookup.EnvelopePost - 1;
        private readonly float[] _ampBuffer = new float[EnvelopeAmp];

        private readonly float[] _nearBuffer = new float[15];
        private int _ampPointer;
        private float _nearDcAcc;
        private float _nearDcPartialAcc;

        private int _nearPointer;

        public float SpreadNearDc(float input)
        {
            // the accumulation is regularly refreshed from scratch to avoid floating point creep 
            if (_nearPointer == 0)
            {
                _nearDcAcc = _nearDcPartialAcc + input;
                _nearDcPartialAcc = input;
            }
            else
            {
                _nearDcAcc += input;
                _nearDcPartialAcc += input;
            }

            _nearDcAcc -= _nearBuffer[_nearPointer];
            _nearBuffer[_nearPointer] = input;

            var decay = _nearDcAcc;
            decay *= (float) (1.0/(_nearBuffer.Length + 1));

            _nearPointer++;
            if (_nearPointer >= _nearBuffer.Length)
                _nearPointer = 0;

            return (float) (decay.ToDecibel()*.5 - 15f);
        }

        public Delta ConvertAmplitudeToDelta(float amplitude, int stretch)
        {
            float preMax = -99999f, preMin = 99999f;

            var p = _ampPointer;
            if (--p < 0)
                p += EnvelopeAmp;

            var postMax = Math.Max(amplitude, _ampBuffer[p]);
            var postMin = Math.Min(amplitude, _ampBuffer[p]);

            for (var i = 0; i < stretch; i++)
            {
                if (--p < 0)
                    p += EnvelopeAmp;

                preMax = Math.Max(preMax, _ampBuffer[p]);
                preMin = Math.Min(preMin, _ampBuffer[p]);
            }

            _ampBuffer[_ampPointer] = amplitude;
            _ampPointer++;

            if (_ampPointer >= _ampBuffer.Length)
                _ampPointer = 0;

            return new Delta(
                postMin - preMin,
                postMax - preMax);
        }
    }
}