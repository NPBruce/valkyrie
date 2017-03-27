namespace OggVorbisEncoder.Setup
{
    public class ISetupTemplate
    {
        public int Mappings;
        public double[] SampleRateMapping;
        public double[] QualityMapping;
        public int CouplingRestriction;
        public int SampleRateMinRestriction;
        public int SampleRateMaxRestriction;
        public int[] BlockSizeShort;
        public int[] BlockSizeLong;
        public Att3[] PsyToneMasterAtt;
        public int[] PsyTone0Decibel;
        public int[] PsyToneDecibelSuppress;
        public AdjBlock[] PsyToneAdjImpulse;
        public AdjBlock[] PsyToneAdjLong;
        public AdjBlock[] PsyToneAdjOther;
        public NoiseGuard[] PsyNoiseGuards;
        public Noise3[] PsyNoiseBiasImpulse;
        public Noise3[] PsyNoiseBiasPadding;
        public Noise3[] PsyNoiseBiasTrans;
        public Noise3[] PsyNoiseBiasLong;
        public int[] PsyNoiseDecibelSuppress;
        public CompandBlock[] PsyNoiseCompand;
        public double[] PsyNoiseCompandShortMapping;
        public double[] PsyNoiseCompandLongMapping;
        public int[][] PsyNoiseNormalStart;
        public int[][] PsyNoiseNormalPartition;
        public double[] PsyNoiseNormalThreshold;
        public int[] PsyAthFloat;
        public int[] PsyAthAbs;
        public double[] PsyLowPass;
        public PsyGlobal[] GlobalParams;
        public double[] GlobalMapping;
        public AdjStereo[] StereoModes;
        public IStaticCodeBook[][] FloorBooks;
        public Floor[] FloorParams;
        public int[][] FloorMappings;
        public IMappingTemplate[] Maps;
    }
}