namespace Inner_Maps.Location_Structures;

public abstract class AnimalDen : NaturalStructureWithStructureObject
{
	public AnimalDen(STRUCTURE_TYPE structureType, Region location)
		: base(structureType, location)
	{
	}

	public AnimalDen(Region location, SaveDataNaturalStructureWithStructureObject data)
		: base(location, data)
	{
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		if (base.structureType.IsBeastDen())
		{
			LinkThisStructureToVillages();
		}
	}

	private void LinkThisStructureToVillages()
	{
		Area area = base.occupiedArea;
		for (int i = 0; i < base.region.villageSpots.Count; i++)
		{
			VillageSpot villageSpot = base.region.villageSpots[i];
			Area coreSpot = villageSpot.coreSpot;
			if (area.IsNearbyTo(coreSpot))
			{
				villageSpot.AddLinkedBeastDen(this);
			}
		}
	}
}
