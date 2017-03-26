namespace OggVorbisEncoder.Setup
{
    public class NoiseGuard
    {
        public NoiseGuard(int low, int high, int fix)
        {
            Low = low;
            High = high;
            Fixed = fix;
        }

        public int Low { get; }
        public int High { get; }
        public int Fixed { get; }
    }
}