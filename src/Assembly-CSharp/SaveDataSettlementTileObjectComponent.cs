using System;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

[Serializable]
public class SaveDataSettlementTileObjectComponent : SaveData<SettlementTileObjectComponent>
{
	public Point[] wardLightLocations;

	public string[] wardLightIDs;

	public override void Save(SettlementTileObjectComponent data)
	{
		base.Save(data);
		wardLightLocations = new Point[data.wardLightLocations.Count];
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		list.AddRange(data.wardLightLocations);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			Point point = new Point(locationGridTile.localPlace.x, locationGridTile.localPlace.y);
			wardLightLocations[i] = point;
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		wardLightIDs = new string[data.wardLights.Count];
		for (int j = 0; j < data.wardLights.Count; j++)
		{
			WardLight wardLight = data.wardLights[j];
			wardLightIDs[j] = wardLight.persistentID;
		}
	}

	public override SettlementTileObjectComponent Load()
	{
		return new SettlementTileObjectComponent(this);
	}
}
