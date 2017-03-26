namespace OggVorbisEncoder.Setup
{
    public class ISetupTemplate
    {
        int Mappings;
        double[] SampleRateMapping;
        double[] QualityMapping;
        int CouplingRestriction;
        int SampleRateMinRestriction;
        int SampleRateMaxRestriction;
        int[] BlockSizeShort;
        int[] BlockSizeLong;
        Att3[] PsyToneMasterAtt;
        int[] PsyTone0Decibel;
        int[] PsyToneDecibelSuppress;
        AdjBlock[] PsyToneAdjImpulse;
        AdjBlock[] PsyToneAdjLong;
        AdjBlock[] PsyToneAdjOther;
        NoiseGuard[] PsyNoiseGuards;
        Noise3[] PsyNoiseBiasImpulse;
        Noise3[] PsyNoiseBiasPadding;
        Noise3[] PsyNoiseBiasTrans;
        Noise3[] PsyNoiseBiasLong;
        int[] PsyNoiseDecibelSuppress;
        CompandBlock[] PsyNoiseCompand;
        double[] PsyNoiseCompandShortMapping;
        double[] PsyNoiseCompandLongMapping;
        int[][] PsyNoiseNormalStart;
        int[][] PsyNoiseNormalPartition;
        double[] PsyNoiseNormalThreshold;
        public int[] PsyAthFloat;
        public int[] PsyAthAbs;
        public double[] PsyLowPass;
        PsyGlobal[] GlobalParams;
        double[] GlobalMapping;
        AdjStereo[] StereoModes;
        IStaticCodeBook[][] FloorBooks;
        Floor[] FloorParams;
        int[][] FloorMappings;
        IMappingTemplate[] Maps;
    }
}