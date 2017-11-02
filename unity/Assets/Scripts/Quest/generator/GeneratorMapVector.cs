using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorMapVector
{
    public float x = 0;
    public float y = 0;
    public int rotation = 0;

    public GeneratorMapVector()
    {
    }

    public GeneratorMapVector(float xIn, float yIn, int rotationIn = 0)
    {
        x = xIn;
        y = yIn;
        rotation = rotationIn;
    }

    public GeneratorMapVector Rotate(int degrees)
    {
        if (degrees == 90)
        {
            return new GeneratorMapVector(-y, x, degrees + rotation);
        }

        if (degrees == 180)
        {
            return new GeneratorMapVector(-x, -y, degrees + rotation);
        }

        if (degrees == 270)
        {
            return new GeneratorMapVector(y, -x, degrees + rotation);
        }
        return vector;
    }

    public GeneratorMapVector Add(GeneratorMapVector toAdd)
    {
        
        GeneratorMapVector rotated = toAdd.Rotate(rotation);

        return new GeneratorMapVector(rotated.x + x, rotated.y + y, rotated.rotation);
    }
}
