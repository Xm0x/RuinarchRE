using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace Scenario_Maps;

[Serializable]
public class ScenarioMapData
{
	public ScenarioWorldMapSave worldMapSave;

	public SettlementTemplate[] villageSettlementTemplates;

	public void SaveVillageSettlements(List<NPCSettlement> villageSettlements)
	{
		villageSettlementTemplates = new SettlementTemplate[villageSettlements.Count];
		for (int i = 0; i < villageSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement = villageSettlements[i];
			Point[] array = new Point[nPCSettlement.areas.Count];
			for (int j = 0; j < array.Length; j++)
			{
				Area area = nPCSettlement.areas[j];
				array[j] = new Point(area.areaData.xCoordinate, area.areaData.yCoordinate);
			}
			List<StructureSetting> list = new List<StructureSetting>();
			for (int k = 0; k < nPCSettlement.allStructures.Count; k++)
			{
				LocationStructure locationStructure = nPCSettlement.allStructures[k];
				if (locationStructure.structureType.IsVillageStructure() && locationStructure.structureType != STRUCTURE_TYPE.DWELLING && locationStructure is ManMadeStructure manMadeStructure)
				{
					StructureSetting item = new StructureSetting(locationStructure.structureType, manMadeStructure.wallsAreMadeOf.GetResourceForWall());
					list.Add(item);
				}
			}
			villageSettlementTemplates[i] = new SettlementTemplate(array, list.ToArray(), 0, nPCSettlement.owner.race, nPCSettlement.settlementType.settlementType);
		}
	}
}
