namespace OggVorbisEncoder.Setup
{
    public interface ISetupTemplate
    {
        int Mappings { get; }
        double[] SampleRateMapping { get; }
        double[] QualityMapping { get; }
        int CouplingRestriction { get; }
        int SampleRateMinRestriction { get; }
        int SampleRateMaxRestriction { get; }
        int[] BlockSizeShort { get; }
        int[] BlockSizeLong { get; }
        Att3[] PsyToneMasterAtt { get; }
        int[] PsyTone0Decibel { get; }
        int[] PsyToneDecibelSuppress { get; }
        AdjBlock[] PsyToneAdjImpulse { get; }
        AdjBlock[] PsyToneAdjLong { get; }
        AdjBlock[] PsyToneAdjOther { get; }
        NoiseGuard[] PsyNoiseGuards { get; }
        Noise3[] PsyNoiseBiasImpulse { get; }
        Noise3[] PsyNoiseBiasPadding { get; }
        Noise3[] PsyNoiseBiasTrans { get; }
        Noise3[] PsyNoiseBiasLong { get; }
        int[] PsyNoiseDecibelSuppress { get; }
        CompandBlock[] PsyNoiseCompand { get; }
        double[] PsyNoiseCompandShortMapping { get; }
        double[] PsyNoiseCompandLongMapping { get; }
        int[][] PsyNoiseNormalStart { get; }
        int[][] PsyNoiseNormalPartition { get; }
        double[] PsyNoiseNormalThreshold { get; }
        int[] PsyAthFloat { get; }
        int[] PsyAthAbs { get; }
        double[] PsyLowPass { get; }
        PsyGlobal[] GlobalParams { get; }
        double[] GlobalMapping { get; }
        AdjStereo[] StereoModes { get; }
        IStaticCodeBook[][] FloorBooks { get; }
        Floor[] FloorParams { get; }
        int[][] FloorMappings { get; }
        IMappingTemplate[] Maps { get; }
    }
}