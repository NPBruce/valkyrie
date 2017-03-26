namespace OggVorbisEncoder.Setup
{
    public class AdjStereo
    {
        public AdjStereo(
            int[] pre,
            int[] post,
            float[] kilohertz,
            float[] lowPassKilohertz)
        {
            Pre = pre;
            Post = post;
            Kilohertz = kilohertz;
            LowPassKilohertz = lowPassKilohertz;
        }

        public int[] Pre { get; }
        public int[] Post { get; }
        public float[] Kilohertz { get; }
        public float[] LowPassKilohertz { get; }
    }
}