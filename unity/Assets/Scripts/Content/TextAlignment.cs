using System;
using ValkyrieTools;

namespace Assets.Scripts.Content
{
    public enum TextAlignment
    {
        TOP,
        CENTER,
        BOTTOM
    }

    public class TextAlignmentUtils
    {
        public static TextAlignment ParseAlignment(string textAlignString)
        {
            TextAlignment alignment;
            try
            {
                alignment = (TextAlignment) Enum.Parse(typeof(TextAlignment), textAlignString, true);
            }
            catch (ArgumentException e)
            {
                ValkyrieDebug.Log("Failed to parse text alignment - " + textAlignString + ": " + e.Message);
                alignment = TextAlignment.CENTER;
            }

            return alignment;
        }
        
    }
    
}

