using System;
using BayatGames.SaveGameFree.Types;
using Locations.Region_Components;

[Serializable]
public class SaveDataRegion : SaveData<Region>
{
	public string persistentID;

	public int id;

	public string name;

	public int coreTileID;

	public ColorSave regionColor;

	public string[] residentIDs;

	public string[] charactersAtLocationIDs;

	public SaveDataInnerMap innerMapSave;

	public SaveDataVillageSpot[] villageSpots;

	public string[] factionsHereIDs;

	public SaveDataRegionDivisionComponent regionDivisionComponent;

	public SaveDataGridTileFeatureComponent gridTileFeatureComponent;

	public SaveDataRegionTileObjectsComponent regionTileObjectsComponent;

	public override void Save(Region region)
	{
		persistentID = region.persistentID;
		id = region.id;
		name = region.name;
		coreTileID = region.coreTile.id;
		regionColor = region.regionColor;
		residentIDs = new string[region.residents.Count];
		for (int i = 0; i < region.residents.Count; i++)
		{
			Character character = region.residents[i];
			residentIDs[i] = character.persistentID;
		}
		charactersAtLocationIDs = new string[region.charactersAtLocation.Count];
		for (int j = 0; j < region.charactersAtLocation.Count; j++)
		{
			Character character2 = region.charactersAtLocation[j];
			charactersAtLocationIDs[j] = character2.persistentID;
		}
		innerMapSave = new SaveDataInnerMap();
		innerMapSave.Save(region.innerMap);
		factionsHereIDs = new string[region.factionsHere.Count];
		for (int k = 0; k < region.factionsHere.Count; k++)
		{
			Faction faction = region.factionsHere[k];
			factionsHereIDs[k] = faction.persistentID;
		}
		villageSpots = new SaveDataVillageSpot[region.villageSpots.Count];
		for (int l = 0; l < region.villageSpots.Count; l++)
		{
			VillageSpot data = region.villageSpots[l];
			SaveDataVillageSpot saveDataVillageSpot = new SaveDataVillageSpot();
			saveDataVillageSpot.Save(data);
			villageSpots[l] = saveDataVillageSpot;
		}
		regionDivisionComponent = new SaveDataRegionDivisionComponent();
		regionDivisionComponent.Save(region.biomeDivisionComponent);
		gridTileFeatureComponent = new SaveDataGridTileFeatureComponent();
		gridTileFeatureComponent.Save(region.gridTileFeatureComponent);
		regionTileObjectsComponent = new SaveDataRegionTileObjectsComponent();
		regionTileObjectsComponent.Save(region.tileObjectsComponent);
	}

	public override void CleanUp()
	{
		residentIDs = null;
		charactersAtLocationIDs = null;
		innerMapSave.CleanUp();
		innerMapSave = null;
		for (int i = 0; i < villageSpots.Length; i++)
		{
			villageSpots[i].CleanUp();
		}
		villageSpots = null;
		factionsHereIDs = null;
		regionDivisionComponent.CleanUp();
		regionDivisionComponent = null;
		gridTileFeatureComponent.CleanUp();
		gridTileFeatureComponent = null;
		regionTileObjectsComponent.CleanUp();
		regionTileObjectsComponent = null;
	}
}
