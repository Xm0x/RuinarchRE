using Inner_Maps;
using UnityEngine;

public class LocationGridTileOtherData : OtherData
{
	public LocationGridTile tile { get; private set; }

	public override object obj => tile;

	public LocationGridTileOtherData(LocationGridTile tile)
	{
		this.tile = tile;
		if (tile == null)
		{
			Debug.LogWarning("New LocationGridTileOtherData was created but provided tile was null, this is handled but weird.");
		}
	}

	public LocationGridTileOtherData(SaveDataLocationGridTileOtherData saveData)
	{
		if (saveData.tileID.hasValue)
		{
			tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveData.tileID);
		}
	}

	public override SaveDataOtherData Save()
	{
		SaveDataLocationGridTileOtherData saveDataLocationGridTileOtherData = new SaveDataLocationGridTileOtherData();
		saveDataLocationGridTileOtherData.Save(this);
		return saveDataLocationGridTileOtherData;
	}

	public override void CleanUp()
	{
		tile = null;
	}
}
