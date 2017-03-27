using System;
using System.Linq;

namespace OggVorbisEncoder.Setup
{
    public class Mapping
    {
        public Mapping(
            int[] channelMuxList,
            int[] floorSubMap,
            int[] residueSubMap,
            int[] couplingMag,
            int[] couplingAng)
        {
            if (floorSubMap != null && residueSubMap != null && floorSubMap.Length != residueSubMap.Length)
                throw new ArgumentException("{nameof(floorSubMap)} and {nameof(residueSubMap)} must be the same size");

            if (couplingMag != null && couplingAng != null && couplingMag.Length != couplingAng.Length)
                throw new ArgumentException("{nameof(couplingMag)} and {nameof(couplingAng)} must be the same size");

            ChannelMuxList = channelMuxList;
            FloorSubMap = floorSubMap;
            ResidueSubMap = residueSubMap;
            CouplingMag = couplingMag;
            CouplingAng = couplingAng;
        }

        public int SubMaps { get { return FloorSubMap.Length; } }

        public int[] ChannelMuxList; // up to 256 channels in a Vorbis stream

        public int[] FloorSubMap; // [mux] submap to floors
        public int[] ResidueSubMap; // [mux] submap to residue

        public int CouplingSteps { get { return CouplingMag.Length; } }

        public int[] CouplingMag;
        public int[] CouplingAng;

        public Mapping Clone()
        {
            return new Mapping(
            ChannelMuxList.ToArray(),
            FloorSubMap.ToArray(),
            ResidueSubMap.ToArray(),
            CouplingMag.ToArray(),
            CouplingAng.ToArray());
        }
    }
}