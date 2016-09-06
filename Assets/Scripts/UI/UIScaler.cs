using UnityEngine;
using System.Collections;

// I couldn't work out unity scaling so I wrote my own.
// The screen is alway 30 'units' high.  At 4:3 it is 40 across, at 16:9 it is 53.33.
// I have not designed for 5:4 (37.5 units)
// 1 unit is enough for 'small' text with a border.  1.5 is medium text, 3 is big text
// Everything is floats so partial units are fine.

public class UIScaler {

    public int widthPx;
    public int heightPx;
    public static float rowsOfUnits = 30;

    public UIScaler(Canvas c)
    {
        widthPx = Mathf.RoundToInt(c.transform.position.x * 2);
        heightPx = Mathf.RoundToInt(c.transform.position.y * 2);
    }

    public static float GetPixelsPerUnit()
    {
        Game game = Game.Get();
        return (float)game.uiScaler.heightPx / rowsOfUnits;
    }

    public static Vector2 Location(float x, float y)
    {
        return new Vector2(x * GetPixelsPerUnit(), y * GetPixelsPerUnit());
    }

    public static float GetWidthUnits()
    {
        Game game = Game.Get();
        return (float)game.uiScaler.widthPx / GetPixelsPerUnit();
    }

    public static float GetHeightUnits()
    {
        Game game = Game.Get();
        return (float)game.uiScaler.heightPx / GetPixelsPerUnit();
    }

    public static float GetRight(float offset = 0)
    {
        return GetWidthUnits() + offset;
    }

    public static float GetBottom(float offset = 0)
    {
        return GetHeightUnits() + offset;
    }

    public static float GetVCenter(float offset = 0)
    {
        return (rowsOfUnits / 2) + offset;
    }

    public static float GetHCenter(float offset = 0)
    {
        return (GetWidthUnits() / 2) + offset;
    }

    public static int GetSmallFont()
    {
        return Mathf.RoundToInt(GetPixelsPerUnit() * 0.8f);
    }
    public static int GetMediumFont()
    {
        return Mathf.RoundToInt(GetPixelsPerUnit() * 1.2f);
    }
    public static int GetLargeFont()
    {
        return Mathf.RoundToInt(GetPixelsPerUnit() * 2.4f);
    }
}
