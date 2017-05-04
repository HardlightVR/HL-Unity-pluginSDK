using UnityEngine;
using System.Collections;

public static class Texture2DExtension
{
	public static Texture2D Colorize(this Texture2D texture, Color colorizeValue)
	{
		Color[] pixels = texture.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] += colorizeValue;
		}
		texture.SetPixels(pixels);
		Texture2D newTexture = texture;
		return newTexture;
	}
}
