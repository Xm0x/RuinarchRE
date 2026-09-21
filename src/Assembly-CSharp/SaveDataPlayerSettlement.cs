using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;

public class SaveDataPlayerSettlement : SaveDataBaseSettlement
{
	public List<TileLocationSave> corruptedTiles;

	public override void Save(BaseSettlement data)
	{
		base.Save(data);
		if (data is PlayerSettlement playerSettlement)
		{
			corruptedTiles = new List<TileLocationSave>();
			for (int i = 0; i < playerSettlement.corruptedTiles.Count; i++)
			{
				LocationGridTile locationGridTile = playerSettlement.corruptedTiles[i];
				TileLocationSave item = new TileLocationSave(locationGridTile);
				corruptedTiles.Add(item);
			}
		}
	}

	public override BaseSettlement Load()
	{
		return LandmarkManager.Instance.LoadPlayerSettlement(this);
	}
}
