using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorMapJoint
{
    // Location is always the center of the joint between tiles
    public GeneratorMapVector location;
    public char type;

    public GeneratorMapJoint(GeneratorMapVectory locationIn, char typeIn)
    {
        location = locationIn;
        type = typeIn;
    }
}
