public class SaveDataFishingSpot : SaveDataTileObject
{
	public string connectedFishingShackID;

	public override void Save(TileObject data)
	{
		base.Save(data);
		FishingSpot fishingSpot = data as FishingSpot;
		if (fishingSpot.connectedFishingShack != null)
		{
			connectedFishingShackID = fishingSpot.connectedFishingShack.persistentID;
		}
	}
}
