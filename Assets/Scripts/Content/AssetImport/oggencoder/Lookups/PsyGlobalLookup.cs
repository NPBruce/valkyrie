using System;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{
    public class PsyGlobalLookup
    {
        private const int NegativeInfinite = -9999;
        private readonly PsyGlobal _psyGlobal;
        private float _ampMax;

        public PsyGlobalLookup(PsyGlobal global)
        {
            _psyGlobal = global;
            AmpMax = NegativeInfinite;
        }

        public float AmpMax
        {
            get { return _ampMax; }
            private set { _ampMax = Math.Max(NegativeInfinite, value); }
        }

        public void DecayAmpMax(int n, int sampleRate)
        {
            var secs = (float) n/sampleRate;
            AmpMax += secs*_psyGlobal.AmpMaxAttPerSec;
        }
    }
}