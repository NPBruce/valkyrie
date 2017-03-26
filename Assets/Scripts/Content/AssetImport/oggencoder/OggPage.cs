namespace OggVorbisEncoder
{
    /// <summary>
    ///     Encapsulates the data for an Ogg page.
    /// </summary>
    /// <remarks>
    ///     Ogg pages are the fundamental unit of framing and interleave in an ogg
    ///     bitstream. They are made up of packet segments of 255 bytes each. There
    ///     can be as many as 255 packet segments per page, for a maximum page size
    ///     of a little under 64 kB. This is not a practical limitation as the
    ///     segments can be joined across page boundaries allowing packets of
    ///     arbitrary size. In practice pages are usually around 4 kB.
    /// </remarks>
    public class OggPage
    {
        public OggPage(
            byte[] header,
            byte[] body)
        {
            Header = header;
            Body = body;
        }

        /// <summary>
        ///     The page header for this page. The exact contents of this header
        ///     are defined in the framing spec document.
        /// </summary>
        public byte[] Header { get; }

        /// <summary>
        ///     The data for this page.
        /// </summary>
        public byte[] Body { get; }
    }
}