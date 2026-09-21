using Inner_Maps.Location_Structures;

public class SaveDataFishery : SaveDataManMadeStructure
{
	public string connectedFishingShackID;

	public string connectedFishingSpotID;

	public bool hasTargetFishingLocation;

	public Point targetFishingLocation;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		Fishery fishery = locationStructure as Fishery;
		if (fishery.connectedOcean != null)
		{
			connectedFishingShackID = fishery.connectedOcean.persistentID;
		}
		if (fishery.connectedFishingSpot != null)
		{
			connectedFishingSpotID = fishery.connectedFishingSpot.persistentID;
		}
		if (fishery.targetFishingLocation != null)
		{
			hasTargetFishingLocation = true;
			targetFishingLocation = new Point(fishery.targetFishingLocation.localPlace.x, fishery.targetFishingLocation.localPlace.y);
		}
		else
		{
			hasTargetFishingLocation = false;
		}
	}
}
