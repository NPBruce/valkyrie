using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Content;

public class GeneratorTileJoin
{
    public int x, y, rotation;
    public char type;

    public GeneratorTileJoin(int xIn, int yIn, char typeIn)
    {
        x = xIn;
        y = yIn;
        type = typeIn;
    }

    // Odd numbers are 'A' type joins
    public bool IsTypeADerivative()
    {
        return (System.Convert.ToByte(type) % 2) == 1;
    }

    public int TypeAAsSign()
    {
        if (IsTypeADerivative) return 1;
        return -1;
    }
}
