namespace OggVorbisEncoder
{
    /// <summary>
    ///     Encapsulates the data for a single raw packet of data
    /// </summary>
    public class OggPacket
    {
        public OggPacket(
            byte[] packetData,
            bool endOfStream,
            int granulePosition,
            int packetNumber)
        {
            PacketData = packetData;
            EndOfStream = endOfStream;
            GranulePosition = granulePosition;
            PacketNumber = packetNumber;
        }

        /// <summary>
        ///     This is treated as an opaque type by the ogg layer.
        /// </summary>
        public byte[] PacketData { get; }

        /// <summary>
        ///     Flag indicating whether this packet ends a bitstream.
        /// </summary>
        public bool EndOfStream { get; }

        /// <summary>
        ///     A number indicating the position of this packet in the decoded
        ///     data. This is the last sample, frame or other unit of information
        ///     ('granule') that can be completely decoded from this packet.
        /// </summary>
        public int GranulePosition { get; }

        /// <summary>
        ///     Sequential number of this packet in the ogg bitstream.
        /// </summary>
        public int PacketNumber { get; }
    }
}