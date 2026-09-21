using System;
using UnityEngine;

[Serializable]
public class SaveDataChaosOrb
{
	public Vector3 pos;

	public string regionID;

	public void Save(ChaosOrb orb)
	{
		pos = orb.randomPos;
		regionID = orb.location.persistentID;
	}

	public void Load()
	{
		Region mainRegion = DatabaseManager.Instance.regionDatabase.mainRegion;
		PlayerManager.Instance.CreateChaosOrbFromSave(pos, mainRegion);
	}
}
