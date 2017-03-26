using System;
using System.Linq;

namespace OggVorbisEncoder
{
    internal static class ArrayExtensions
    {
        public static TElement[] ToFixedLength<TElement>(this TElement[] input, int fixedLength)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Length == fixedLength)
                return input;

            if (input.Length > fixedLength)
                throw new IndexOutOfRangeException(
                    $"{nameof(input)} of size [{input.Length}] is greater than {nameof(fixedLength)} of [{fixedLength}]");

            var missingCount = fixedLength - input.Length;
            var blankElements = Enumerable.Range(0, missingCount).Select(s => default(TElement));

            return input
                .Concat(blankElements)
                .ToArray();
        }
    }
}