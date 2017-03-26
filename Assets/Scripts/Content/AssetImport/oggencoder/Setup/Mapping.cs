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
            if (floorSubMap?.Length != residueSubMap?.Length)
                throw new ArgumentException("{nameof(floorSubMap)} and {nameof(residueSubMap)} must be the same size");

            if (couplingMag?.Length != couplingAng?.Length)
                throw new ArgumentException("{nameof(couplingMag)} and {nameof(couplingAng)} must be the same size");

            ChannelMuxList = channelMuxList;
            FloorSubMap = floorSubMap;
            ResidueSubMap = residueSubMap;
            CouplingMag = couplingMag;
            CouplingAng = couplingAng;
        }

        public int SubMaps => FloorSubMap.Length;

        public int[] ChannelMuxList { get; } // up to 256 channels in a Vorbis stream

        public int[] FloorSubMap { get; } // [mux] submap to floors
        public int[] ResidueSubMap { get; } // [mux] submap to residue

        public int CouplingSteps => CouplingMag.Length;

        public int[] CouplingMag { get; }
        public int[] CouplingAng { get; }

        public Mapping Clone() => new Mapping(
            ChannelMuxList.ToArray(),
            FloorSubMap.ToArray(),
            ResidueSubMap.ToArray(),
            CouplingMag.ToArray(),
            CouplingAng.ToArray());
    }
}