using System;

namespace OggVorbisEncoder.Lookups
{
    public class EnvelopeBand
    {
        public EnvelopeBand(
            int begin,
            int windowLength)
        {
            Begin = begin;
            Window = new float[windowLength];

            for (var i = 0; i < Window.Length; i++)
            {
                Window[i] = (float) Math.Sin((i + .5)/Window.Length*Math.PI);
                Total += Window[i];
            }

            Total = (float) (1.0/Total);
        }

        public int Begin { get; }
        public float[] Window { get; }
        public float Total { get; }
    }
}