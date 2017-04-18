﻿using System.Linq;

namespace OggVorbisEncoder.Setup
{
    public class PsyGlobal
    {
        public const int EnvelopeBands = 7;
        public const int PacketBlobs = 15;

        public PsyGlobal(
            int eighthOctaveLines,
            float[] preEchoThreshold,
            float[] postEchoThreshold,
            float stretchPenalty,
            float preEchoMinEnergy,
            float ampMaxAttPerSecond,
            int[] couplingPerKilohertz,
            int[][] couplingPointLimit,
            int[] couplingPrePointAmp,
            int[] couplingPostPointAmp,
            int[][] slidingLowPass)
        {
            EighthOctaveLines = eighthOctaveLines;

            PreEchoThreshold = preEchoThreshold.ToFixedLength(EnvelopeBands);
            PostEchoThreshold = postEchoThreshold.ToFixedLength(EnvelopeBands);

            StretchPenalty = stretchPenalty;
            PreEchoMinEnergy = preEchoMinEnergy;
            AmpMaxAttPerSec = ampMaxAttPerSecond;

            CouplingPerKilohertz = couplingPerKilohertz.ToFixedLength(PacketBlobs);
            CouplingPrePointAmp = couplingPrePointAmp.ToFixedLength(PacketBlobs);
            CouplingPostPointAmp = couplingPostPointAmp.ToFixedLength(PacketBlobs);

            CouplingPointLimit = couplingPointLimit.Select(s => s.ToFixedLength(PacketBlobs)).ToArray();
            SlidingLowPass = slidingLowPass.Select(s => s.ToFixedLength(PacketBlobs)).ToArray();
        }

        public int EighthOctaveLines;

        // for block long/short tuning; encode only 
        public float[] PreEchoThreshold;
        public float[] PostEchoThreshold;
        public float StretchPenalty;
        public float PreEchoMinEnergy;
        public float AmpMaxAttPerSec { get; set; }

        // channel coupling config 
        public int[] CouplingPerKilohertz;
        public int[][] CouplingPointLimit;
        public int[] CouplingPrePointAmp { get; set; }
        public int[] CouplingPostPointAmp { get; set; }
        public int[][] SlidingLowPass;

        public PsyGlobal Clone()
        {
            return new PsyGlobal(
            EighthOctaveLines,
            PreEchoThreshold.ToArray(),
            PostEchoThreshold.ToArray(),
            StretchPenalty,
            PreEchoMinEnergy,
            AmpMaxAttPerSec,
            CouplingPerKilohertz.ToArray(),
            CouplingPointLimit.Select(s => s.ToArray()).ToArray(),
            CouplingPrePointAmp.ToArray(),
            CouplingPostPointAmp.ToArray(),
            SlidingLowPass.Select(s => s.ToArray()).ToArray());
        }
    }
}