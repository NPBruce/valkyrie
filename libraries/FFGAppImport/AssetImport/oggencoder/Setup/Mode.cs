namespace OggVorbisEncoder.Setup
{
    public struct Mode
    {
        public int BlockFlag;
        public int WindowType;
        public int TransformType;
        public int Mapping;

        public Mode(
            int blockFlag,
            int windowType,
            int transformType,
            int mapping)
        {
            BlockFlag = blockFlag;
            WindowType = windowType;
            TransformType = transformType;
            Mapping = mapping;
        }
    }
}