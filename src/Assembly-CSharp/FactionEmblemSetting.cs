using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class FactionEmblemSetting
{
	public FactionEmblemDictionary emblems;

	public Sprite GetSpriteForSize(Image image)
	{
		if (image.rectTransform.sizeDelta.x <= 24f)
		{
			return emblems[24];
		}
		return emblems[96];
	}

	public Sprite GetSpriteForSize(int size)
	{
		if (size <= 24)
		{
			return emblems[24];
		}
		return emblems[96];
	}
}
