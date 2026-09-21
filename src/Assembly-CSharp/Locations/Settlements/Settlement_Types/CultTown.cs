using Inner_Maps.Location_Structures;

namespace Locations.Settlements.Settlement_Types;

public class CultTown : SettlementType
{
	public CultTown()
		: base(SETTLEMENT_TYPE.Cult_Town)
	{
		base.maxDwellings = 10;
		base.maxFacilities = 16;
	}

	public CultTown(SaveDataSettlementType saveData)
		: base(saveData)
	{
		base.maxDwellings = 10;
		base.maxFacilities = 16;
	}

	public override void ApplyDefaultSettings()
	{
		SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE), 300, 1);
		SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.CULT_TEMPLE, RESOURCE.STONE), 80, 1);
		SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE), 50, 1);
		SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE), 30, 1);
		SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 10, 1);
		SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE), 20, 1);
		SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.HOSPICE, RESOURCE.STONE), 10, 1);
	}

	public override StructureSetting GetDwellingSetting(Faction faction)
	{
		return new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE);
	}
}
