using System;
using UnityEngine;

[Serializable]
public struct LandmarkStructureSprite
{
	public Sprite mainSprite;

	public Sprite tintSprite;

	public RuntimeAnimatorController animation;

	public Sprite overrideLandmarkPortrait;

	public static LandmarkStructureSprite Empty => new LandmarkStructureSprite
	{
		mainSprite = null,
		tintSprite = null,
		animation = null
	};

	public LandmarkStructureSprite(Sprite mainSprite, Sprite tintSprite, RuntimeAnimatorController animation = null)
	{
		this.mainSprite = mainSprite;
		this.tintSprite = tintSprite;
		this.animation = animation;
		overrideLandmarkPortrait = null;
	}
}
