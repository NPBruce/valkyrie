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

    public GeneratorMapJoint(GeneratorMapJoint in)
    {
        location = new GeneratorMapVector(in.location);
        type = in.type;
    }

    public bool MatingJoint(GeneratorMapJoint j)
    {
        if (j.location.x - location.x > 0.1f) return false;
        if (j.location.x - location.x < -0.1f) return false;
        if (j.location.y - location.y > 0.1f) return false;
        if (j.location.y - location.y < -0.1f) return false;
        if (((j.rotation + rotation) % 360) != 0) return false;
        return (type == j.type);
    }
}
