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

        public int[] Pre;
        public int[] Post;
        public float[] Kilohertz;
        public float[] LowPassKilohertz;
    }
}