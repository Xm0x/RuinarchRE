using System;

[Serializable]
public struct POIData
{
	public string poiID;

	public POINT_OF_INTEREST_TYPE poiType;

	public POIData(IPointOfInterest p_poi)
	{
		poiID = p_poi.persistentID;
		poiType = p_poi.poiType;
	}
}
