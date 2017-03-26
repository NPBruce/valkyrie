using System.Linq;

namespace OggVorbisEncoder.Setup
{
    public class PsyInfo
    {
        public const int Bands = 17;
        private const int NoiseCompandLevels = 40;
        private const int NoiseCurves = 3;
        private float[] _noiseCompand;
        private float[][] _noiseOffset;
        private float[] _toneAtt;

        private float[] _toneMasterAtt;

        public PsyInfo(
            int blockFlag,
            float athAdjAtt,
            float athMaxAtt,
            float[] toneMasterAtt,
            float toneCenterBoost,
            float toneDecay,
            float toneAbsLimit,
            float[] toneAtt,
            int noiseMaskP,
            float noiseMaxSuppress,
            float noiseWindowLow,
            float noiseWindowHigh,
            int noiseWindowLowMin,
            int noiseWindowHighMin,
            int noiseWindowFixed,
            float[][] noiseOffset,
            float[] noiseCompand,
            float maxCurveDecibel,
            bool normalize,
            int normalStart,
            int normalPartition,
            double normalThreshold)
        {
            BlockFlag = blockFlag;
            AthAdjAtt = athAdjAtt;
            AthMaxAtt = athMaxAtt;
            ToneMasterAtt = toneMasterAtt;
            ToneCenterBoost = toneCenterBoost;
            ToneDecay = toneDecay;
            ToneAbsLimit = toneAbsLimit;
            ToneAtt = toneAtt;
            NoiseMaskP = noiseMaskP;
            NoiseMaxSuppress = noiseMaxSuppress;
            NoiseWindowLow = noiseWindowLow;
            NoiseWindowHigh = noiseWindowHigh;
            NoiseWindowLowMin = noiseWindowLowMin;
            NoiseWindowHighMin = noiseWindowHighMin;
            NoiseWindowFixed = noiseWindowFixed;
            NoiseOffset = noiseOffset;
            NoiseCompand = noiseCompand;
            MaxCurveDecibel = maxCurveDecibel;
            Normalize = normalize;
            NormalStart = normalStart;
            NormalPartition = normalPartition;
            NormalThreshold = normalThreshold;
        }

        public int BlockFlag { get; set; }

        public float AthAdjAtt { get; set; }
        public float AthMaxAtt { get; set; }

        public float[] ToneMasterAtt
        {
            get { return _toneMasterAtt; }
            private set { _toneMasterAtt = value.ToFixedLength(NoiseCurves); }
        }

        public float ToneCenterBoost { get; set; }
        public float ToneDecay { get; set; }
        public float ToneAbsLimit { get; set; }

        public float[] ToneAtt
        {
            get { return _toneAtt; }
            private set { _toneAtt = value.ToFixedLength(Bands); }
        }

        public int NoiseMaskP { get; }
        public float NoiseMaxSuppress { get; set; }
        public float NoiseWindowLow { get; }
        public float NoiseWindowHigh { get; }
        public int NoiseWindowLowMin { get; set; }
        public int NoiseWindowHighMin { get; set; }
        public int NoiseWindowFixed { get; set; }

        public float[][] NoiseOffset
        {
            get { return _noiseOffset; }
            private set
            {
                var fixedValue = value.Select(s => s.ToFixedLength(Bands).ToArray());
                _noiseOffset = fixedValue.ToArray().ToFixedLength(NoiseCurves);
            }
        }

        public float[] NoiseCompand
        {
            get { return _noiseCompand; }
            private set { _noiseCompand = value.ToFixedLength(NoiseCompandLevels); }
        }

        public float MaxCurveDecibel { get; set; }

        public bool Normalize { get; set; }
        public int NormalStart { get; set; }
        public int NormalPartition { get; set; }
        public double NormalThreshold { get; set; }

        public PsyInfo Clone() => new PsyInfo(
            BlockFlag,
            AthAdjAtt,
            AthMaxAtt,
            ToneMasterAtt.ToArray(),
            ToneCenterBoost,
            ToneDecay,
            ToneAbsLimit,
            ToneAtt.ToArray(),
            NoiseMaskP,
            NoiseMaxSuppress,
            NoiseWindowLow,
            NoiseWindowHigh,
            NoiseWindowLowMin,
            NoiseWindowHighMin,
            NoiseWindowFixed,
            NoiseOffset.Select(s => s.ToArray()).ToArray(),
            NoiseCompand.ToArray(),
            MaxCurveDecibel,
            Normalize,
            NormalStart,
            NormalPartition,
            NormalThreshold);
    }
}