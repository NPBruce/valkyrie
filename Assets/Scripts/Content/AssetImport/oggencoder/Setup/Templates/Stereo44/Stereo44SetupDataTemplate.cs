namespace OggVorbisEncoder.Setup.Templates.Stereo44
{
    public class Stereo44SetupDataTemplate : ISetupTemplate
    {
        private static readonly double[] SampleRateMapping44Stereo =
        {
            22500, 32000, 40000, 48000, 56000, 64000, 80000,
            96000, 112000, 128000, 160000, 250001
        };

        private static readonly double[] QualityMapping44 = {-.1, .0, .1, .2, .3, .4, .5, .6, .7, .8, .9, 1.0};
        private static readonly int[] BlocksizeShort44 = {512, 256, 256, 256, 256, 256, 256, 256, 256, 256, 256};

        private static readonly int[] BlocksizeLong44 =
        {
            4096, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,
            2048
        };

        private static readonly int[] NoiseStartShort44 = {32, 16, 16, 16, 32, 9999, 9999, 9999, 9999, 9999, 9999};
        private static readonly int[] NoiseStartLong44 = {256, 128, 128, 256, 512, 9999, 9999, 9999, 9999, 9999, 9999};
        private static readonly int[] NoisePartShort44 = {8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8};
        private static readonly int[] NoisePartLong44 = {32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32};
        private static readonly double[] NoiseThresh44 = {.2, .2, .2, .4, .6, 9999, 9999, 9999, 9999, 9999, 9999};

        private static readonly int[] FloorMapping44A = {1, 0, 0, 2, 2, 4, 5, 5, 5, 5, 5};
        private static readonly int[] FloorMapping44B = {8, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7};
        //private static readonly int[] FloorMapping44C ={10,10,10,10,10,10,10,10,10,10,10};
        private static readonly int[][] FloorMapping44 =
        {
            FloorMapping44A,
            FloorMapping44B
            //FloorMapping44C, 
        };

        private static readonly AdjBlock[] VpToneMaskAdjLongBlock =
        {
            new AdjBlock(new[] {-3, -8, -13, -15, -10, -10, -10, -10, -10, -10, -10, 0, 0, 0, 0, 0, 0}), /* -1 */
            new AdjBlock(new[] {-4, -10, -14, -16, -15, -14, -13, -12, -12, -12, -11, -1, -1, -1, -1, -1, 0}), /* 0 */
            new AdjBlock(new[] {-6, -12, -14, -16, -15, -15, -14, -13, -13, -12, -12, -2, -2, -1, -1, -1, 0}), /* 1 */
            new AdjBlock(new[] {-12, -13, -14, -16, -16, -16, -15, -14, -13, -12, -12, -6, -3, -1, -1, -1, 0}), /* 2 */
            new AdjBlock(new[] {-15, -15, -15, -16, -16, -16, -16, -14, -13, -13, -13, -10, -4, -2, -1, -1, 0}), /* 3 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -13, -11, -7 - 3, -1, -1, 0}), /* 4 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -13, -11, -7 - 3, -1, -1, 0}), /* 5 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -8, -4, -2, -2, 0}), /* 6 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 7 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 8 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 9 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}) /* 10 */
        };

        private static readonly AdjBlock[] VpToneMaskAdjOtherBlock =
        {
            new AdjBlock(new[] {-3, -8, -13, -15, -10, -10, -9, -9, -9, -9, -9, 1, 1, 1, 1, 1, 1}), /* -1 */
            new AdjBlock(new[] {-4, -10, -14, -16, -14, -13, -12, -12, -11, -11, -10, 0, 0, 0, 0, 0, 0}), /* 0 */
            new AdjBlock(new[] {-6, -12, -14, -16, -15, -15, -14, -13, -13, -12, -12, -2, -2, -1, 0, 0, 0}), /* 1 */
            new AdjBlock(new[] {-12, -13, -14, -16, -16, -16, -15, -14, -13, -12, -12, -5, -2, -1, 0, 0, 0}), /* 2 */
            new AdjBlock(new[] {-15, -15, -15, -16, -16, -16, -16, -14, -13, -13, -13, -10, -4, -2, 0, 0, 0}), /* 3 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -13, -11, -7 - 3, -1, -1, 0}), /* 4 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -13, -11, -7 - 3, -1, -1, 0}), /* 5 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -8, -4, -2, -2, 0}), /* 6 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 7 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 8 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 9 */
            new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}) /* 10 */
        };

        public int Mappings { get { return SampleRateMapping.Length - 1; } }
        public double[] SampleRateMapping = SampleRateMapping44Stereo;
        public double[] QualityMapping = QualityMapping44;
        public int CouplingRestriction = 2;
        public int SampleRateMinRestriction = 40000;
        public int SampleRateMaxRestriction = 50000;
        public int[] BlockSizeShort = BlocksizeShort44;
        public int[] BlockSizeLong = BlocksizeLong44;
        public Att3[] PsyToneMasterAtt = Psy44.ToneMasterAtt;
        public int[] PsyTone0Decibel = Psy.ToneZeroDecibel;
        public int[] PsyToneDecibelSuppress = Psy.ToneSuppress;
        public AdjBlock[] PsyToneAdjImpulse = VpToneMaskAdjOtherBlock;
        public AdjBlock[] PsyToneAdjLong = VpToneMaskAdjLongBlock;
        public AdjBlock[] PsyToneAdjOther = VpToneMaskAdjOtherBlock;
        public NoiseGuard[] PsyNoiseGuards = Psy44.NoiseGuards;
        public Noise3[] PsyNoiseBiasImpulse = Psy.NoiseBiasImpulseValues;
        public Noise3[] PsyNoiseBiasPadding = Psy.NoiseBiasPaddingValues;
        public Noise3[] PsyNoiseBiasTrans = Psy.NoiseBiasTransition;
        public Noise3[] PsyNoiseBiasLong = Psy.NoiseBiasLongBlock;
        public int[] PsyNoiseDecibelSuppress = Psy.NoiseSuppress;
        public CompandBlock[] PsyNoiseCompand = Psy44.Compand;
        public double[] PsyNoiseCompandShortMapping = Psy.CompandShortMapping;
        public double[] PsyNoiseCompandLongMapping = Psy.CompandLongMapping;
        public int[][] PsyNoiseNormalStart = {NoiseStartShort44, NoiseStartLong44};
        public int[][] PsyNoiseNormalPartition = {NoisePartShort44, NoisePartLong44};
        public double[] PsyNoiseNormalThreshold = NoiseThresh44;
        new public int[] PsyAthFloat = Psy.AthFloater;
        new public int[] PsyAthAbs = Psy44.AthAbs;
        new public double[] PsyLowPass = Psy44.Lowpass;
        public PsyGlobal[] GlobalParams = Psy44.Global;
        public double[] GlobalMapping = Psy44.GlobalMapping;
        public AdjStereo[] StereoModes = Psy44.StereoModes;
        public IStaticCodeBook[][] FloorBooks = SharedFloors.FloorBooks;
        public Floor[] FloorParams = SharedFloors.Floor;
        public int[][] FloorMappings = FloorMapping44;
        public IMappingTemplate[] Maps = Residue44.MapRes44Stereo;
    }
}