using System.Collections.Generic;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{
    public class CodecSetup
    {
        public CodecSetup(EncodeSetup encodeSetup)
        {
            EncodeSetup = encodeSetup;
        }

        public EncodeSetup EncodeSetup;

        public int[] BlockSizes = new int[2];

        public CodeBook[] FullBooks { get; set; }
        public IList<IStaticCodeBook> BookParams = new List<IStaticCodeBook>();
        public IList<Mode> ModeParams = new List<Mode>();
        public IList<Mapping> MapParams = new List<Mapping>();
        public IList<Floor> FloorParams = new List<Floor>();
        public IList<Residue> ResidueParams = new List<Residue>();
        public IList<PsyInfo> PsyParams = new List<PsyInfo>();
        public PsyGlobal PsyGlobalParam { get; set; }
    }
}