using System.Collections.Generic;
using System.Linq;
using OggVorbisEncoder.Setup;
using OggVorbisEncoder.Setup.Templates;

namespace OggVorbisEncoder
{
    public class VorbisInfo
    {
        private static readonly Mode[] ModeTemplate = {new Mode(0, 0, 0, 0), new Mode(1, 0, 0, 1)};

        public VorbisInfo(
            CodecSetup codecSetup,
            int channels,
            int sampleRate,
            int bitRateNominal)
        {
            CodecSetup = codecSetup;
            Channels = channels;
            SampleRate = sampleRate;
            BitRateNominal = bitRateNominal;
        }

        public int Channels;

        public int SampleRate;

        public int BitRateNominal;

        public CodecSetup CodecSetup;

        public static VorbisInfo InitVariableBitRate(int channels, int sampleRate, float baseQuality)
        {
            var encodeSetup = GetEncodeSetup(channels, sampleRate, baseQuality);
            var codecSetup = new CodecSetup(encodeSetup);
            var template = encodeSetup.Template;

            // choose Block sizes from configured sizes as well as paying
            // attention to long_Block_p and short_Block_p.  If the configured
            // short and long Blocks are the same length, we set long_Block_p
            // and unset short_Block_p 
            BlockSizeSetup(
                codecSetup,
                (int) encodeSetup.BaseSetting,
                new List<int>(template.BlockSizeShort),
                new List<int>(template.BlockSizeLong));

            var singleBlock = codecSetup.BlockSizes[0]
                              == codecSetup.BlockSizes[1];

            // floor setup; choose proper floor params.  Allocated on the floor
            // stack in order; if we alloc only long floor, it's 0 
            foreach (var floorMappings in template.FloorMappings)
                FloorSetup(
                    codecSetup,
                    (int) encodeSetup.BaseSetting,
                    new List<IStaticCodeBook[]>(template.FloorBooks),
                    new List<Floor>(template.FloorParams),
                    new List<int>(floorMappings));

            // setup of [mostly] short Block detection and stereo
            GlobalPsychSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                new List<PsyGlobal>(template.GlobalParams),
                new List<double>(template.GlobalMapping));

            GlobalStereo(
                codecSetup,
                sampleRate,
                new List<AdjStereo>(template.StereoModes));

            // basic psych setup and noise normalization 
            PsyParamSetup(
                codecSetup,
                (int) encodeSetup.BaseSetting,
                new List<int>(template.PsyNoiseNormalStart[0]),
                new List<int>(template.PsyNoiseNormalPartition[0]),
                new List<double>(template.PsyNoiseNormalThreshold));

            PsyParamSetup(
                codecSetup,
                (int) encodeSetup.BaseSetting,
                new List<int>(template.PsyNoiseNormalStart[0]),
                new List<int>(template.PsyNoiseNormalPartition[0]),
                new List<double>(template.PsyNoiseNormalThreshold));

            if (!singleBlock)
            {
                PsyParamSetup(
                    codecSetup,
                    (int) encodeSetup.BaseSetting,
                    new List<int>(template.PsyNoiseNormalStart[1]),
                    new List<int>(template.PsyNoiseNormalPartition[1]),
                    new List<double>(template.PsyNoiseNormalThreshold));

                PsyParamSetup(
                    codecSetup,
                    (int) encodeSetup.BaseSetting,
                    new List<int>(template.PsyNoiseNormalStart[1]),
                    new List<int>(template.PsyNoiseNormalPartition[1]),
                    new List<double>(template.PsyNoiseNormalThreshold));
            }

            // tone masking setup 
            ToneMaskSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                0,
                new List<Att3>(template.PsyToneMasterAtt),
                new List<int>(template.PsyTone0Decibel),
                new List<AdjBlock>(template.PsyToneAdjImpulse));

            ToneMaskSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                1,
                new List<Att3>(template.PsyToneMasterAtt),
                new List<int>(template.PsyTone0Decibel),
                new List<AdjBlock>(template.PsyToneAdjOther));

            if (!singleBlock)
            {
                ToneMaskSetup(
                    codecSetup,
                    encodeSetup.BaseSetting,
                    2,
                    new List<Att3>(template.PsyToneMasterAtt),
                    new List<int>(template.PsyTone0Decibel),
                    new List<AdjBlock>(template.PsyToneAdjOther));

                ToneMaskSetup(
                    codecSetup,
                    encodeSetup.BaseSetting,
                    3,
                    new List<Att3>(template.PsyToneMasterAtt),
                    new List<int>(template.PsyTone0Decibel),
                    new List<AdjBlock>(template.PsyToneAdjLong));
            }

            // noise compand setup 
            CompandSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                0,
                new List<CompandBlock>(template.PsyNoiseCompand),
                new List<double>(template.PsyNoiseCompandShortMapping));

            CompandSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                1,
                new List<CompandBlock>(template.PsyNoiseCompand),
                new List<double>(template.PsyNoiseCompandShortMapping));

            if (!singleBlock)
            {
                CompandSetup(
                    codecSetup,
                    encodeSetup.BaseSetting,
                    2,
                    new List<CompandBlock>(template.PsyNoiseCompand),
                    new List<double>(template.PsyNoiseCompandLongMapping));

                CompandSetup(
                    codecSetup,
                    encodeSetup.BaseSetting,
                    3,
                    new List<CompandBlock>(template.PsyNoiseCompand),
                    new List<double>(template.PsyNoiseCompandLongMapping));
            }

            // peak guarding setup  
            PeakSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                0,
                new List<int>(template.PsyToneDecibelSuppress));

            PeakSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                1,
                new List<int>(template.PsyToneDecibelSuppress));

            if (!singleBlock)
            {
                PeakSetup(
                    codecSetup,
                    encodeSetup.BaseSetting,
                    2,
                    new List<int>(template.PsyToneDecibelSuppress));

                PeakSetup(
                    codecSetup,
                    encodeSetup.BaseSetting,
                    3,
                    new List<int>(template.PsyToneDecibelSuppress));
            }

            // noise bias setup 
            NoiseBiasSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                0,
                new List<int>(template.PsyNoiseDecibelSuppress),
                new List<Noise3>(template.PsyNoiseBiasImpulse),
                new List<NoiseGuard>(template.PsyNoiseGuards));

            NoiseBiasSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                1,
                new List<int>(template.PsyNoiseDecibelSuppress),
                new List<Noise3>(template.PsyNoiseBiasPadding),
                new List<NoiseGuard>(template.PsyNoiseGuards));

            if (!singleBlock)
            {
                NoiseBiasSetup(
                    codecSetup,
                    encodeSetup.BaseSetting,
                    2,
                    new List<int>(template.PsyNoiseDecibelSuppress),
                    new List<Noise3>(template.PsyNoiseBiasTrans),
                    new List<NoiseGuard>(template.PsyNoiseGuards));

                NoiseBiasSetup(
                    codecSetup,
                    encodeSetup.BaseSetting,
                    3,
                    new List<int>(template.PsyNoiseDecibelSuppress),
                    new List<Noise3>(template.PsyNoiseBiasLong),
                    new List<NoiseGuard>(template.PsyNoiseGuards));
            }

            AthSetup(codecSetup, 0);
            AthSetup(codecSetup, 1);

            if (!singleBlock)
            {
                AthSetup(codecSetup, 2);
                AthSetup(codecSetup, 3);
            }

            MapAndResSetup(
                codecSetup,
                sampleRate,
                channels,
                new List<IMappingTemplate>(template.Maps));

            var bitRateNominal = GetApproxBitRate(encodeSetup, channels);

            return new VorbisInfo(
                codecSetup,
                channels,
                sampleRate,
                bitRateNominal);
        }

        private static void PsyParamSetup(
            CodecSetup codecSetup,
            int encodeSetupBaseSetting,
            List<int> noiseNormalStart,
            List<int> noiseNormalPartition,
            List<double> noiseNormalThreshold)
        {
            var block = codecSetup.PsyParams.Count;

            var psyParam = Psy.PsyInfoTemplate.Clone();
            codecSetup.PsyParams.Add(psyParam);
            psyParam.BlockFlag = block >> 1;

            psyParam.Normalize = true;
            psyParam.NormalStart = noiseNormalStart[encodeSetupBaseSetting];
            psyParam.NormalPartition = noiseNormalPartition[encodeSetupBaseSetting];
            psyParam.NormalThreshold = noiseNormalThreshold[encodeSetupBaseSetting];
        }

        private static void FloorSetup(
            CodecSetup codecSetup,
            int encodeSetupBaseSetting,
            List<IStaticCodeBook[]> templateFloorBooks,
            List<Floor> templateFloorParams,
            List<int> templateFloorMappings)
        {
            var sourceIndex = templateFloorMappings[encodeSetupBaseSetting];
            var clonedFloor = templateFloorParams[sourceIndex].Clone();

            // books 
            int maxClass = -1, maxBook = -1;

            foreach (var partitionClass in clonedFloor.PartitionClass)
                if (partitionClass > maxClass)
                    maxClass = partitionClass;

            for (var i = 0; i <= maxClass; i++)
            {
                if (clonedFloor.ClassBook[i] > maxBook)
                    maxBook = clonedFloor.ClassBook[i];

                clonedFloor.ClassBook[i] += codecSetup.BookParams.Count;

                for (var k = 0; k < 1 << clonedFloor.ClassSubs[i]; k++)
                {
                    if (clonedFloor.ClassSubBook[i][k] > maxBook)
                        maxBook = clonedFloor.ClassSubBook[i][k];

                    if (clonedFloor.ClassSubBook[i][k] >= 0)
                        clonedFloor.ClassSubBook[i][k] += codecSetup.BookParams.Count;
                }
            }

            for (var i = 0; i <= maxBook; i++)
            {
                var bookParam = templateFloorBooks[sourceIndex][i];
                codecSetup.BookParams.Add(bookParam);
            }

            codecSetup.FloorParams.Add(clonedFloor);
        }

        private static void MapAndResSetup(
            CodecSetup codecSetup,
            int sampleRate,
            int channels,
            List<IMappingTemplate> templateMaps)
        {
            var encodeSetupBaseSetting = (int) codecSetup.EncodeSetup.BaseSetting;
            var map = templateMaps[encodeSetupBaseSetting].Mapping;
            var res = templateMaps[encodeSetupBaseSetting].ResidueTemplate;

            var modes = 2;
            if (codecSetup.BlockSizes[0] == codecSetup.BlockSizes[1])
                modes = 1;

            for (var i = 0; i < modes; i++)
            {
                codecSetup.ModeParams.Add(ModeTemplate[i]);
                codecSetup.MapParams.Add(map[i].Clone());

                for (var j = 0; j < map[i].SubMaps; j++)
                    ResidueSetup(
                        codecSetup,
                        sampleRate,
                        channels,
                        map[i].ResidueSubMap[j],
                        i,
                        res[map[i].ResidueSubMap[j]]);
            }
        }

        private static void ResidueSetup(
            CodecSetup codecSetup,
            int sampleRate,
            int channels,
            int number,
            int block,
            IResidueTemplate residueTemplate)
        {
            var residue = residueTemplate.Residue.Clone(
                residueTemplate.ResidueType,
                residueTemplate.Grouping);

            codecSetup.ResidueParams.Add(residue);

            // fill in all the books
            FillBooks(codecSetup, residue, residueTemplate.BookAux, residueTemplate.BooksBase);

            // lowpass setup/pointlimit 
            var freq = codecSetup.EncodeSetup.LowPassKilohertz*1000;
            var f = codecSetup.FloorParams[block]; // by convention
            var nyq = sampleRate/2.0;
            var blocksize = codecSetup.BlockSizes[block] >> 1;

            // lowpass needs to be set in the floor and the residue. 
            if (freq > nyq)
                freq = nyq;

            // in the floor, the granularity can be very fine; it doesn't alter
            // the encoding structure, only the samples used to fit the floor approximation 
            f.N = (int) (freq/nyq*blocksize);

            // this res may by limited by the maximum pointlimit of the mode,
            // not the lowpass. the floor is always lowpass limited.
            switch (residueTemplate.LimitType)
            {
                case ResidueLimitType.PointStereo:
                    freq = codecSetup.PsyGlobalParam.CouplingPerKilohertz[PsyGlobal.PacketBlobs/2]*1000;

                    if (freq > nyq)
                        freq = nyq;

                    break;

                case ResidueLimitType.LowFrequencyEffects:
                    freq = 250;
                    break;
            }

            // in the residue, we're constrained, physically, by partition
            // boundaries.  We still lowpass 'wherever', but we have to round up
            // here to next boundary, or the vorbis spec will round it *down* to
            // previous boundary in encode/decode
            if (residue.ResidueType == ResidueType.Two)
            {
                // Residue 2 bundles together multiple channels; used by stereo
                // and surround.  Count the channels in use 
                // Multiple maps/submaps can point to the same residue.  In the case
                // of residue 2, they all better have the same number of channels/samples. 
                var ch = 0;
                for (var i = 0; (i < codecSetup.MapParams.Count) && (ch == 0); i++)
                {
                    var mapping = codecSetup.MapParams[i];
                    for (var j = 0; (j < mapping.SubMaps) && (ch == 0); j++)
                        if (mapping.ResidueSubMap[j] == number) // we found a submap referencing this residue backend 
                            for (var k = 0; k < channels; k++)
                                if (mapping.ChannelMuxList[k] == j) // this channel belongs to the submap 
                                    ch++;
                }

                // round up only if we're well past
                residue.End = (int) (freq/nyq*blocksize*ch/residue.Grouping + .9)*residue.Grouping;

                // the blocksize and grouping may disagree at the end
                if (residue.End > blocksize*ch)
                    residue.End = blocksize*ch/residue.Grouping*residue.Grouping;
            }
            else
            {
                // round up only if we're well past
                residue.End = (int) (freq/nyq*blocksize/residue.Grouping + .9)*residue.Grouping;

                // the blocksize and grouping may disagree at the end 
                if (residue.End > blocksize)
                    residue.End = blocksize/residue.Grouping*residue.Grouping;
            }

            if (residue.End == 0)
                residue.End = residue.Grouping; // LFE channel 
        }

        private static void FillBooks(
            CodecSetup codecSetup,
            Residue r,
            IStaticCodeBook bookAux,
            IStaticBookBlock bookBlock)
        {
            for (var i = 0; i < r.Partitions; i++)
                for (var k = 0; k < 4; k++)
                    if ((i < bookBlock.Books.Length)
                        && (k < bookBlock.Books[i].Length)
                        && (bookBlock.Books[i][k] != null))
                        r.SecondStages[i] |= 1 << k;

            r.GroupBook = GetOrAddBook(codecSetup, bookAux);

            var booklist = 0;
            for (var i = 0; i < r.Partitions; i++)
                for (var k = 0; k < 4; k++)
                    if ((i < bookBlock.Books.Length)
                        && (k < bookBlock.Books[i].Length))
                    {
                        var sourceBook = bookBlock.Books[i][k];
                        if (sourceBook != null)
                        {
                            var bookid = GetOrAddBook(codecSetup, sourceBook);
                            r.BookList[booklist++] = bookid;
                        }
                    }
        }

        private static int GetOrAddBook(CodecSetup codecSetup, IStaticCodeBook codeBook)
        {
            int i;
            for (i = 0; i < codecSetup.BookParams.Count; i++)
                if (codecSetup.BookParams[i] == codeBook)
                    return i;

            codecSetup.BookParams.Add(codeBook);
            return codecSetup.BookParams.Count - 1;
        }

        private static void GlobalPsychSetup(
            CodecSetup codecSetup,
            double encodeSetupTriggerSetting,
            List<PsyGlobal> templateGlobalParams,
            List<double> templateGlobalMapping)
        {
            var setting = (int) encodeSetupTriggerSetting;
            var ds = encodeSetupTriggerSetting - setting;

            var sourceIndex = (int) templateGlobalMapping[setting];
            var globalParam = codecSetup.PsyGlobalParam = templateGlobalParams[sourceIndex].Clone();

            ds = templateGlobalMapping[setting]
                 *(1 - ds) + templateGlobalMapping[setting + 1]
                 *ds;

            setting = (int) ds;
            ds -= setting;
            if ((ds <= 0) && (setting > 0))
            {
                setting--;
                ds = 1;
            }

            // interpolate the trigger threshholds 
            for (var i = 0; i < 4; i++)
            {
                globalParam.PreEchoThreshold[i] = (float)
                (templateGlobalParams[setting].PreEchoThreshold[i]*(1 - ds)
                 + templateGlobalParams[setting + 1].PreEchoThreshold[i]*ds);

                globalParam.PostEchoThreshold[i] = (float)
                (templateGlobalParams[setting].PostEchoThreshold[i]*(1 - ds)
                 + templateGlobalParams[setting + 1].PostEchoThreshold[i]*ds);
            }

            globalParam.AmpMaxAttPerSec = (float) codecSetup.EncodeSetup.AmplitudeTrackDbPerSec;
        }

        private static void GlobalStereo(
            CodecSetup codecSetup,
            int sampleRate,
            List<AdjStereo> templateStereoModes)
        {
            var setting = (int) codecSetup.EncodeSetup.BaseSetting;
            var ds = codecSetup.EncodeSetup.BaseSetting - setting;
            var psyGlobal = codecSetup.PsyGlobalParam;
            var packetBlobs = PsyGlobal.PacketBlobs;

            if (templateStereoModes != null)
            {
                psyGlobal.CouplingPrePointAmp = templateStereoModes[setting].Pre.ToArray();
                psyGlobal.CouplingPostPointAmp = templateStereoModes[setting].Post.ToArray();

                var kHz = templateStereoModes[setting].Kilohertz[packetBlobs/2]*(1 - ds)
                          + templateStereoModes[setting + 1].Kilohertz[packetBlobs/2]*ds;

                for (var i = 0; i < packetBlobs; i++)
                {
                    psyGlobal.CouplingPointLimit[0][i] =
                        (int) (kHz*1000/sampleRate*codecSetup.BlockSizes[0]);
                    psyGlobal.CouplingPointLimit[1][i] =
                        (int) (kHz*1000/sampleRate*codecSetup.BlockSizes[1]);
                    psyGlobal.CouplingPerKilohertz[i] = (int) kHz;
                }

                kHz = templateStereoModes[setting].LowPassKilohertz[packetBlobs/2]*(1 - ds)
                      + templateStereoModes[setting + 1].LowPassKilohertz[packetBlobs/2]*ds;

                for (var i = 0; i < packetBlobs; i++)
                {
                    psyGlobal.SlidingLowPass[0][i] = (int) (kHz*1000/sampleRate*codecSetup.BlockSizes[0]);
                    psyGlobal.SlidingLowPass[1][i] = (int) (kHz*1000/sampleRate*codecSetup.BlockSizes[1]);
                }
            }
            else
            {
                for (var i = 0; i < packetBlobs; i++)
                {
                    psyGlobal.SlidingLowPass[0][i] = codecSetup.BlockSizes[0];
                    psyGlobal.SlidingLowPass[1][i] = codecSetup.BlockSizes[1];
                }
            }
        }

        private static void BlockSizeSetup(
            CodecSetup codecSetup,
            int index,
            List<int> templateBlockSizeShort,
            List<int> templateBlockSizeLong)
        {
            var blockshort = templateBlockSizeShort[index];
            var blocklong = templateBlockSizeLong[index];
            codecSetup.BlockSizes[0] = blockshort;
            codecSetup.BlockSizes[1] = blocklong;
        }

        private static void ToneMaskSetup(
            CodecSetup codecSetup,
            double toneMaskSetting,
            int block,
            List<Att3> templatePsyToneMasterAtt,
            List<int> templatePsyTone0Decibel,
            List<AdjBlock> templatePsyToneAdjLong)
        {
            var setting = (int) toneMaskSetting;
            var ds = toneMaskSetting - setting;

            var psyParam = codecSetup.PsyParams[block];

            // 0 and 2 are only used by bitmanagement, but there's no harm to always filling the values in here
            psyParam.ToneMasterAtt[0] = (float) (templatePsyToneMasterAtt[setting].Att[0]*(1 - ds)
                                                 + templatePsyToneMasterAtt[setting + 1].Att[0]*ds);

            psyParam.ToneMasterAtt[1] = (float) (templatePsyToneMasterAtt[setting].Att[1]*(1 - ds)
                                                 + templatePsyToneMasterAtt[setting + 1].Att[1]*ds);

            psyParam.ToneMasterAtt[2] = (float) (templatePsyToneMasterAtt[setting].Att[2]*(1 - ds)
                                                 + templatePsyToneMasterAtt[setting + 1].Att[2]*ds);

            psyParam.ToneCenterBoost = (float) (templatePsyToneMasterAtt[setting].Boost*(1 - ds)
                                                + templatePsyToneMasterAtt[setting + 1].Boost*ds);

            psyParam.ToneDecay = (float) (templatePsyToneMasterAtt[setting].Decay*(1 - ds)
                                          + templatePsyToneMasterAtt[setting + 1].Decay*ds);

            psyParam.MaxCurveDecibel = (float) (templatePsyTone0Decibel[setting]*(1 - ds)
                                                + templatePsyTone0Decibel[setting + 1]*ds);

            for (var i = 0; i < psyParam.ToneAtt.Length; i++)
                psyParam.ToneAtt[i] = (float) (templatePsyToneAdjLong[setting].Block[i]*(1 - ds)
                                               + templatePsyToneAdjLong[setting + 1].Block[i]*ds);
        }

        private static void CompandSetup(
            CodecSetup codecSetup,
            double noiseCompandSetting,
            int block,
            List<CompandBlock> templatePsyNoiseCompand,
            List<double> templatePsyNoiseCompandShortMapping)
        {
            var setting = (int) noiseCompandSetting;
            var ds = noiseCompandSetting - setting;
            var p = codecSetup.PsyParams[block];

            ds = templatePsyNoiseCompandShortMapping[setting]*(1 - ds)
                 + templatePsyNoiseCompandShortMapping[setting + 1]*ds;

            setting = (int) ds;
            ds -= setting;
            if ((ds <= 0) && (setting > 0))
            {
                setting--;
                ds = 1;
            }

            // interpolate the compander settings 
            for (var i = 0; i < p.NoiseCompand.Length; i++)
                p.NoiseCompand[i] = (float) (templatePsyNoiseCompand[setting].Data[i]*(1 - ds)
                                             + templatePsyNoiseCompand[setting + 1].Data[i]*ds);
        }

        private static void PeakSetup(
            CodecSetup codecSetup,
            double tonePeakLimitSetting,
            int block,
            List<int> templatePsyToneDecibelSuppress)
        {
            var setting = (int) tonePeakLimitSetting;
            var ds = tonePeakLimitSetting - setting;
            var p = codecSetup.PsyParams[block];

            p.ToneAbsLimit = (float) (templatePsyToneDecibelSuppress[setting]*(1 - ds)
                                      + templatePsyToneDecibelSuppress[setting + 1]*ds);
        }

        private static void NoiseBiasSetup(
            CodecSetup codecSetup,
            double noiseBiasSetting,
            int block,
            List<int> templatePsyNoiseDecibelSuppress,
            List<Noise3> templatePsyNoiseBiasLong,
            List<NoiseGuard> templatePsyNoiseGuards)
        {
            var setting = (int) noiseBiasSetting;
            var ds = noiseBiasSetting - setting;
            var psyParam = codecSetup.PsyParams[block];

            psyParam.NoiseMaxSuppress = (float) (templatePsyNoiseDecibelSuppress[setting]*(1 - ds)
                                                 + templatePsyNoiseDecibelSuppress[setting + 1]*ds);

            psyParam.NoiseWindowLowMin = templatePsyNoiseGuards[block].Low;
            psyParam.NoiseWindowHighMin = templatePsyNoiseGuards[block].High;
            psyParam.NoiseWindowFixed = templatePsyNoiseGuards[block].Fixed;

            for (var j = 0; j < psyParam.NoiseOffset.Length; j++)
                for (var i = 0; i < psyParam.NoiseOffset[j].Length; i++)
                    psyParam.NoiseOffset[j][i] = (float) (templatePsyNoiseBiasLong[setting].Data[j][i]*(1 - ds)
                                                          + templatePsyNoiseBiasLong[setting + 1].Data[j][i]*ds);

            // impulse blocks may take a user specified bias to boost the nominal/high noise encoding depth
            foreach (var noiseOffset in psyParam.NoiseOffset)
            {
                var min = noiseOffset[0] + 6;
                for (var i = 0; i < noiseOffset.Length; i++)
                    if (noiseOffset[i] < min)
                        noiseOffset[i] = min;
            }
        }

        private static void AthSetup(CodecSetup codecSetup, int block)
        {
            var psyParam = codecSetup.PsyParams[block];

            psyParam.AthAdjAtt = (float) codecSetup.EncodeSetup.AthFloatingDecibel;
            psyParam.AthMaxAtt = (float) codecSetup.EncodeSetup.AthAbsoluteDecibel;
        }

        private static EncodeSetup GetEncodeSetup(
            int channels,
            int sampleRate,
            float quality)
        {
            quality += .0000001f;
            if (quality >= 1)
                quality = .9999f;

            return EncodeSetup.GetBestMatch(
                channels,
                sampleRate,
                quality);
        }

        private static int GetApproxBitRate(EncodeSetup encodeSetup, int channels)
        {
            var template = encodeSetup.Template;

            var setting = (int) encodeSetup.BaseSetting;
            var ds = encodeSetup.BaseSetting - setting;

            if (template.SampleRateMapping == null)
                return -1;

            return (int) ((template.SampleRateMapping[setting]*(1 - ds)
                           + template.SampleRateMapping[setting + 1]*ds)
                          *channels);
        }
    }
}