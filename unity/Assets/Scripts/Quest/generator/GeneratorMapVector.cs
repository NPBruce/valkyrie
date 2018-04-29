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

    public GeneratorMapVector(GeneratorMapVector inVector)
    {
        x = inVector.x;
        y = inVector.y;
        rotation = inVector.rotation;
    }

    /// <summary>
    /// Get a vector which is a copy of this vector rotated about 0,0</summary>
    /// <param name="degrees">degrees to rotate (0/90/180/270)</param>
    /// <returns>New vector</returns>
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
        return new GeneratorMapVector(this);
    }

    /// <summary>
    /// Is this vector within 1 space?</summary>
    /// <param name="compare">Vector to check</param>
    /// <returns>True if within 1 space</returns>
    public bool WithinASpace(GeneratorMapVector compare)
    {
        if (x - compare.x < -0.8f) return false;
        if (x - compare.x > 0.8f) return false;
        if (y - compare.y < -0.8f) return false;
        if (y - compare.y > 0.8f) return false;
        return true;
    }

    /// <summary>
    /// Create new vector which is output of vector addition</summary>
    /// <param name="toAdd">Vector to add</param>
    /// <returns>Resultant vector</returns>
    public GeneratorMapVector Add(GeneratorMapVector toAdd)
    {
        
        GeneratorMapVector rotated = toAdd.Rotate(rotation);

        return new GeneratorMapVector(rotated.x + x, rotated.y + y, rotated.rotation);
    }
}
