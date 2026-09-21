using System.IO;
using UnityEngine;

public static class IMG2Sprite
{
	public static Texture2D LoadTexture(string FilePath)
	{
		if (File.Exists(FilePath))
		{
			byte[] data = File.ReadAllBytes(FilePath);
			Texture2D texture2D = new Texture2D(2, 2);
			if (texture2D.LoadImage(data))
			{
				return texture2D;
			}
		}
		return null;
	}
}
