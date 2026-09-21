using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct LandmarkData
{
	[Header("General Data")]
	public string landmarkTypeString;

	public LANDMARK_TYPE landmarkType;

	public int buildDuration;

	public string description;

	public Sprite landmarkObjectSprite;

	[FormerlySerializedAs("landmarkPortrait")]
	public Sprite defaultLandmarkPortrait;

	public BiomeLandmarkSpriteListDictionary biomeTileSprites;

	public List<LandmarkStructureSprite> neutralTileSprites;

	public List<LandmarkStructureSprite> humansLandmarkTileSprites;

	public List<LandmarkStructureSprite> elvenLandmarkTileSprites;

	public int monsterGenerationChance;

	public MonsterGenerationSetting monsterGenerationSetting;

	public ItemGenerationSetting itemGenerationSetting;

	public void ConstructData()
	{
	}
}
