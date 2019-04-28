namespace Fabric.Internal.Editor.View
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using Fabric.Internal.Editor.Model;
	
	public class Render
	{
		#region Constants
		public static readonly uint InitialWindowHeight = 534;
		public static readonly uint InitialWindowWidth = 322;
		#endregion

		#region Style
		public static Color32 LBlue = FromHex (0x073D65);
		public static Color32 DBlue = FromHex (0x021F35);
		public static Color32 Lerped = Color32.Lerp (LBlue, DBlue, 0.5f);
		#endregion

		public static Color32 FromHex(int hex)
		{
			byte r = (byte)((hex >> 16) & 0xff);
			byte g = (byte)((hex >> 8) & 0xff);
			byte b = (byte)((hex) & 0xff);
			return new Color32 (r, g, b, 255);
		}

		public static Texture2D MakeBackground(int width, int height, Color color)
		{
			Color[] colors = new Color[width * height];
			for (int i = 0; i < colors.Length; ++i)
				colors [i] = color;

			Texture2D retval = new Texture2D (width, height);
			retval.SetPixels (colors);
			retval.Apply ();

			return retval;
		}

		public static void Center(Rect position, GUIStyle style)
		{
			style.margin.left = (int)(position.width / 2 - style.fixedWidth / 2);
		}
	}
}
