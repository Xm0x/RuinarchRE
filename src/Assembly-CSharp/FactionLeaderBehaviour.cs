using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class FactionLeaderBehaviour : CharacterBehaviour
{
	public FactionLeaderBehaviour()
	{
		base.priority = 31;
		attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[1] { BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.behaviourComponent.shouldTryToBuildNewVillage && character.behaviourComponent.chosenVillageSpotForNewVillage != null)
		{
			Area coreSpot = character.behaviourComponent.chosenVillageSpotForNewVillage.coreSpot;
			StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, character.faction.factionType.mainResource);
			GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(character.faction.factionType.type, structureSetting));
			if (LandmarkManager.Instance.HasEnoughSpaceForStructure(randomElement.name, coreSpot.gridTileComponent.centerGridTile) && character.jobComponent.TriggerFindNewVillage(coreSpot.gridTileComponent.centerGridTile, out producedJob, randomElement.name))
			{
				return true;
			}
		}
		if (character.currentRegion.HasStructure(STRUCTURE_TYPE.LEGENDARY_FORGE) && ChanceData.RollChance(CHANCE_TYPE.Claim_Legendary_Forge) && character.homeSettlement != null)
		{
			LocationStructure randomStructureOfType = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.LEGENDARY_FORGE);
			List<Area> list = RuinarchListPool<Area>.Claim();
			randomStructureOfType.occupiedArea.PopulateAreasInRange(list, 6, includeCenterTile: true);
			bool flag = false;
			for (int i = 0; i < character.homeSettlement.areas.Count; i++)
			{
				Area item = character.homeSettlement.areas[i];
				if (list.Contains(item))
				{
					flag = true;
					break;
				}
			}
			RuinarchListPool<Area>.Release(list);
			if (flag)
			{
				TileObject firstTileObjectOfType = randomStructureOfType.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.LEGENDARY_FORGE_TILE_OBJECT);
				if (randomStructureOfType.settlementLocation.owner == null)
				{
					return character.jobComponent.TriggerClaimLegendaryForge(firstTileObjectOfType, out producedJob);
				}
			}
		}
		producedJob = null;
		return false;
	}

	private int GetJobsThatWillBuildFacility(List<JobQueueItem> jobs)
	{
		int num = 0;
		for (int i = 0; i < jobs.Count; i++)
		{
			if (jobs[i] is GoapPlanJob { poiTarget: GenericTileObject poiTarget } && poiTarget.blueprintOnTile != null && poiTarget.blueprintOnTile.structureType.IsFacilityStructure())
			{
				num++;
			}
		}
		return num;
	}

	private int GetJobsThatWillBuildDwelling(List<JobQueueItem> jobs)
	{
		int num = 0;
		for (int i = 0; i < jobs.Count; i++)
		{
			if (jobs[i] is GoapPlanJob { poiTarget: GenericTileObject poiTarget } && poiTarget.blueprintOnTile != null && poiTarget.blueprintOnTile.structureType == STRUCTURE_TYPE.DWELLING)
			{
				num++;
			}
		}
		return num;
	}
}
