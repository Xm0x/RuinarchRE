using System;
using System.Collections.Generic;
using Locations.Settlements;
using UtilityScripts;

[Serializable]
public class SaveDataBaseSettlement : SaveData<BaseSettlement>, ISavableCounterpart
{
	public string _persistentID;

	public int id;

	public LOCATION_TYPE locationType;

	public string name;

	public bool isStoredAsTarget;

	public List<Point> tileCoordinates;

	public string factionOwnerID;

	public List<string> residents;

	public List<string> parties;

	public List<POIData> firesInSettlement;

	public string persistentID => _persistentID;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Settlement;

	public override void Save(BaseSettlement data)
	{
		_persistentID = data.persistentID;
		id = data.id;
		locationType = data.locationType;
		name = data.name;
		isStoredAsTarget = data.isStoredAsTarget;
		residents = SaveUtilities.ConvertSavableListToIDs(data.residents);
		factionOwnerID = ((data.owner != null) ? data.owner.persistentID : string.Empty);
		tileCoordinates = RuinarchListPool<Point>.Claim();
		for (int i = 0; i < data.areas.Count; i++)
		{
			Area area = data.areas[i];
			tileCoordinates.Add(new Point(area.areaData.xCoordinate, area.areaData.yCoordinate));
		}
		parties = RuinarchListPool<string>.Claim();
		for (int j = 0; j < data.parties.Count; j++)
		{
			Party party = data.parties[j];
			parties.Add(party.persistentID);
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(party);
		}
		if (data.firesToDouseInSettlement.Count > 0)
		{
			firesInSettlement = SaveUtilities.ConvertPOIListToPOIData(data.firesToDouseInSettlement);
		}
	}

	public override void CleanUp()
	{
		if (tileCoordinates != null)
		{
			RuinarchListPool<Point>.Release(tileCoordinates);
			tileCoordinates = null;
		}
		if (residents != null)
		{
			RuinarchListPool<string>.Release(residents);
			residents = null;
		}
		if (parties != null)
		{
			RuinarchListPool<string>.Release(parties);
			parties = null;
		}
		if (firesInSettlement != null)
		{
			RuinarchListPool<POIData>.Release(firesInSettlement);
			firesInSettlement = null;
		}
	}
}
