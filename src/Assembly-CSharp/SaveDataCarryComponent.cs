using System;

[Serializable]
public class SaveDataCarryComponent : SaveData<CarryComponent>
{
	public string carriedPOI;

	public POINT_OF_INTEREST_TYPE carriedPOIType;

	public string isBeingCarriedBy;

	public string prevCarriedBy;

	public override void Save(CarryComponent data)
	{
		if (data.carriedPOI != null)
		{
			carriedPOI = data.carriedPOI.persistentID;
			carriedPOIType = data.carriedPOI.poiType;
			if (data.carriedPOI is Character data2)
			{
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data2);
			}
			else if (data.carriedPOI is TileObject data3)
			{
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data3);
			}
		}
		if (data.baseIsBeingCarriedBy != null)
		{
			isBeingCarriedBy = data.baseIsBeingCarriedBy.persistentID;
		}
		if (data.prevCarriedBy != null)
		{
			prevCarriedBy = data.prevCarriedBy.persistentID;
		}
	}

	public override CarryComponent Load()
	{
		return new CarryComponent(this);
	}
}
