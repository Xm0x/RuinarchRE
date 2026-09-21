using System;
using System.Collections.Generic;
using Locations.Area_Features;

[Serializable]
public class SaveDataHextile : SaveData<Area>
{
	public string persistentID;

	public int id;

	public int xCoordinate;

	public int yCoordinate;

	public string tileName;

	public List<SaveDataAreaFeature> areaFeatureSaveData;

	public override void Save(Area tile)
	{
		persistentID = tile.persistentID;
		id = tile.id;
		xCoordinate = tile.areaData.xCoordinate;
		yCoordinate = tile.areaData.yCoordinate;
		tileName = tile.areaData.areaName;
		areaFeatureSaveData = new List<SaveDataAreaFeature>();
		for (int i = 0; i < tile.featureComponent.features.Count; i++)
		{
			AreaFeature areaFeature = tile.featureComponent.features[i];
			SaveDataAreaFeature saveDataAreaFeature = SaveManager.ConvertAreaFeatureToSaveData(areaFeature);
			saveDataAreaFeature.Save(areaFeature);
			areaFeatureSaveData.Add(saveDataAreaFeature);
		}
	}

	public void Load(Area tile)
	{
		if (string.IsNullOrEmpty(persistentID))
		{
			tile.areaData.persistentID = Guid.NewGuid().ToString();
		}
		else
		{
			tile.areaData.persistentID = persistentID;
		}
		tile.areaData.id = id;
		tile.areaData.xCoordinate = xCoordinate;
		tile.areaData.yCoordinate = yCoordinate;
		tile.areaData.areaName = tileName;
	}
}
