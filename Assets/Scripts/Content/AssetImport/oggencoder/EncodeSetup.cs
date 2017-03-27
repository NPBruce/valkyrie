using System;
using System.Collections.Generic;
using OggVorbisEncoder.Setup;
using OggVorbisEncoder.Setup.Templates.Stereo44;

namespace OggVorbisEncoder
{
    public class EncodeSetup
    {
        private static readonly List<ISetupTemplate> SetupTemplates =
            CreateTemplates();

        public EncodeSetup(ISetupTemplate template, double baseSetting)
        {
            Template = template;
            BaseSetting = baseSetting;

            var iS = (int) BaseSetting;
            var ds = BaseSetting - iS;

            LowPassKilohertz = template.PsyLowPass[iS]*(1 - ds)
                               + template.PsyLowPass[iS + 1]*ds;

            AthFloatingDecibel = template.PsyAthFloat[iS]*(1 - ds)
                                 + template.PsyAthFloat[iS + 1]*ds;

            AthAbsoluteDecibel = template.PsyAthAbs[iS]*(1 - ds)
                                 + template.PsyAthAbs[iS + 1]*ds;

            AmplitudeTrackDbPerSec = -6;

            // too low/high an ATH floater is nonsensical, but doesn't break anything 
            if (AthFloatingDecibel > -80)
                AthFloatingDecibel = -80;

            if (AthFloatingDecibel < -200)
                AthFloatingDecibel = -200;

            if (AmplitudeTrackDbPerSec > 0)
                AmplitudeTrackDbPerSec = 0;

            if (AmplitudeTrackDbPerSec < -99999)
                AmplitudeTrackDbPerSec = -99999;
        }

        public ISetupTemplate Template;
        public double BaseSetting;
        public double LowPassKilohertz;
        public double AthFloatingDecibel;
        public double AthAbsoluteDecibel;
        public double AmplitudeTrackDbPerSec;

        public static EncodeSetup GetBestMatch(
            int channels,
            int sampleRate,
            float quality)
        {
            foreach (var template in SetupTemplates)
            {
                if ((template.CouplingRestriction != -1)
                    && (template.CouplingRestriction != channels))
                    continue;

                if ((sampleRate < template.SampleRateMinRestriction)
                    || (sampleRate > template.SampleRateMaxRestriction))
                    continue;

                var map = template.QualityMapping;

                // the template matches.  Does the requested quality mode fall within this template's modes? 
                if ((quality < map[0])
                    || (quality > map[template.Mappings]))
                    continue;

                int j;
                for (j = 0; j < template.Mappings; ++j)
                    if ((quality >= map[j]) && (quality < map[j + 1]))
                        break;

                // an all-points match
                double baseSetting;
                if (j == template.Mappings)
                {
                    baseSetting = j - .001;
                }
                else
                {
                    var low = map[j];
                    var high = map[j + 1];
                    var del = (quality - low)/(high - low);
                    baseSetting = j + del;
                }

                return new EncodeSetup(template, baseSetting);
            }

            throw new InvalidOperationException("No matching template found");
        }

        private static List<ISetupTemplate> CreateTemplates()
        {
            List<ISetupTemplate> list = new List<ISetupTemplate>();
            list.Add(new Stereo44SetupDataTemplate());
            return list;
                //new 44_uncoupled(),
                //new 32_stereo(),
                //new 32_uncoupled(),
                //new 22_stereo(),
                //new 22_uncoupled(),
                //new 16_stereo(),
                //new 16_uncoupled(),
                //new 11_stereo(),
                //new 11_uncoupled(),
                //new 8_stereo(),
                //new 8_uncoupled(),
                //new X_stereo(),
                //new X_uncoupled(),
                //new XX_stereo(),
                //new XX_uncoupled()
        }
    }
}