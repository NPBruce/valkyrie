namespace OggVorbisEncoder.Setup
{
    public class Att3
    {
        public Att3(int[] att, float boost, float decay)
        {
            Att = att;
            Boost = boost;
            Decay = decay;
        }

        public int[] Att;
        public float Boost;
        public float Decay;
    }
}