namespace OggVorbisEncoder.Lookups
{
    public struct Delta
    {
        public Delta(
            float min,
            float max)
        {
            Min = min;
            Max = max;
        }

        public float Min;
        public float Max;
    }
}