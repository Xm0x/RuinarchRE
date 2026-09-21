using System.Collections.Generic;
using UtilityScripts;

public class SaveDataVillageSpot : SaveData<VillageSpot>
{
	public Point mainArea;

	public Point[] reservedAreas;

	public int lumberyardSpots;

	public int miningSpots;

	public List<string> linkedBeastDens;

	public Point migrationSpawningArea;

	public int[] areaDisableVotes;

	public bool isOccupied;

	public override void Save(VillageSpot data)
	{
		base.Save(data);
		mainArea = new Point(data.coreSpot.areaData.xCoordinate, data.coreSpot.areaData.yCoordinate);
		reservedAreas = new Point[data.reservedAreas.Count];
		for (int i = 0; i < data.reservedAreas.Count; i++)
		{
			Area area = data.reservedAreas[i];
			reservedAreas[i] = new Point(area.areaData.xCoordinate, area.areaData.yCoordinate);
		}
		if (data.linkedBeastDens != null && data.linkedBeastDens.Count > 0)
		{
			linkedBeastDens = RuinarchListPool<string>.Claim();
			linkedBeastDens.AddRange(data.linkedBeastDens);
		}
		lumberyardSpots = data.lumberyardSpots;
		miningSpots = data.miningSpots;
		migrationSpawningArea = new Point(data.migrationSpawningArea.areaData.xCoordinate, data.migrationSpawningArea.areaData.yCoordinate);
		areaDisableVotes = data.areaDisableVotes;
		isOccupied = data.isOccupied;
	}

	public override VillageSpot Load()
	{
		return new VillageSpot(this);
	}

	public override void CleanUp()
	{
		reservedAreas = null;
		if (linkedBeastDens != null)
		{
			RuinarchListPool<string>.Release(linkedBeastDens);
			linkedBeastDens = null;
		}
	}
}
