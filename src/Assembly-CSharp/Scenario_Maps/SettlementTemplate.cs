using System;
using Inner_Maps.Location_Structures;

namespace Scenario_Maps;

[Serializable]
public struct SettlementTemplate
{
	public Point[] areas;

	public StructureSetting[] structureSettings;

	public int minimumVillagerCount;

	public RACE factionRace;

	public SETTLEMENT_TYPE settlementType;

	public SettlementTemplate(Point[] areas, StructureSetting[] structureSettings, int minimumVillagerCount, RACE factionRace, SETTLEMENT_TYPE settlementType)
	{
		this.areas = areas;
		this.structureSettings = structureSettings;
		this.minimumVillagerCount = minimumVillagerCount;
		this.factionRace = factionRace;
		this.settlementType = settlementType;
	}

	public Area[] GetTilesInTemplate(Area[,] map)
	{
		Area[] array = new Area[areas.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Point point = areas[i];
			array[i] = map[point.X, point.Y];
		}
		return array;
	}
}
