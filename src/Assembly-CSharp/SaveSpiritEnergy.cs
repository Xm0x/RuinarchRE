using System;
using UnityEngine;

[Serializable]
public class SaveSpiritEnergy
{
	public Vector3 pos;

	public string regionID;

	public void Save(SpiritEnergy orb)
	{
		pos = orb.transform.position;
		regionID = orb.location.persistentID;
	}

	public void Load()
	{
		Region mainRegion = DatabaseManager.Instance.regionDatabase.mainRegion;
		PlayerManager.Instance.CreateSpiritEnergyFromSave(pos, mainRegion);
	}
}
