namespace OggVorbisEncoder.Setup
{
    public interface IStaticCodeBook
    {
        // int Entries { get; } // == LengthList.Length

        /// <summary>
        ///     codebook dimensions (elements per vector)
        /// </summary>
        int Dimensions { get; }

        /// <summary>
        ///     codeword lengths in bits
        /// </summary>
        byte[] LengthList { get; }

        CodeBookMapType MapType { get; }

        /// <summary>
        ///     packed 32 bit float; quant value 0 maps to minval
        /// </summary>
        int QuantMin { get; }

        /// <summary>
        ///     packed 32 bit float; val 1 - val 0 == delta
        /// </summary>
        int QuantDelta { get; }

        /// <summary>
        ///     bits: 0 &lt; quant &lt;= 16
        /// </summary>
        int Quant { get; }

        /// <summary>
        ///     bitflag
        /// </summary>
        int QuantSequenceP { get; }

        /// <summary>
        ///     map == 1: (int)(entries^(1/dim)) element column map
        ///     map == 2: list of dim* entries quantized entry vals
        /// </summary>
        int[] QuantList { get; }
    }
}