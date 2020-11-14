using System.Collections.Generic;
using Assets.Scripts.Content;

public interface IContent
{
    int Priority { get; }
        
    StringKey TranslationKey { get; }
    
    string SectionName { get; }
        
    List<string> Sets { get; }
}