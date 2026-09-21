using UnityEngine;

public class TableTileObjectScriptableObject : TileObjectScriptableObject
{
	[Header("Table Specific")]
	[SerializeField]
	private Sprite defaultTableSprite;

	[SerializeField]
	private Sprite elvenTableSprite;

	public Sprite GetTableSpriteToUseForRace(RACE race)
	{
		if (race == RACE.ELVES)
		{
			return elvenTableSprite;
		}
		return defaultTableSprite;
	}
}
