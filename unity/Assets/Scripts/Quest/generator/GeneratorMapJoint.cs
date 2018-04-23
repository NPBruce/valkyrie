using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorMapJoint
{
    // Location is always the center of the joint between tiles
    public GeneratorMapVector location;
    public char type;

    public GeneratorMapJoint(GeneratorMapVector locationIn, char typeIn)
    {
        location = locationIn;
        type = typeIn;
    }

    public GeneratorMapJoint(GeneratorMapJoint inJoint)
    {
        location = new GeneratorMapVector(inJoint.location);
        type = inJoint.type;
    }

    /// <summary>
    /// Check if joint can mate</summary>
    /// <param name="j">Joint to check</param>
    /// <returns>true if mating matches</returns>
    public bool MatingJoint(GeneratorMapJoint j)
    {
        if (!j.location.WithinASpace(location)) return false;
        if (((j.location.rotation + location.rotation) % 360) != 0) return false;
        return (type == j.type);
    }

    /// <summary>
    /// Check if joint is a Type A derivative (same board side)</summary>
    /// <returns>true if A/C/E etc board side</returns>
    public bool IsTypeADerivative()
    {
        int characterOffset = System.Convert.ToByte(type) - System.Convert.ToByte('A');
        return (characterOffset % 2) == 0;
    }
}
