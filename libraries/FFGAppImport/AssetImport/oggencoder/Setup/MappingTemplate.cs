namespace OggVorbisEncoder.Setup
{
    public class MappingTemplate : IMappingTemplate
    {
        public MappingTemplate(
            Mapping[] mapping,
            IResidueTemplate[] residueTemplate)
        {
            Mapping = mapping;
            ResidueTemplate = residueTemplate;
        }

        public Mapping[] Mapping;
        public IResidueTemplate[] ResidueTemplate;
    }
}