using System.Collections.Generic;

namespace Assets.Scripts.Content
{
    public interface IContent
    {
        int Priority { get; }
        
        List<string> Sets { get; }
    }
}