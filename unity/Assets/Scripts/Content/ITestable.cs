using System.Collections.Generic;

namespace Assets.Scripts.Content
{
    public interface ITestable
    {
        VarTests Tests { get; }
        List<VarOperation> Operations { get; }
    }
}