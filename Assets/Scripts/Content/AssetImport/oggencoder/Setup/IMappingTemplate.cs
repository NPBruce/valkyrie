namespace OggVorbisEncoder.Setup
{
    public interface IMappingTemplate
    {
        Mapping[] Mapping { get; }
        IResidueTemplate[] ResidueTemplate { get; }
    }
}