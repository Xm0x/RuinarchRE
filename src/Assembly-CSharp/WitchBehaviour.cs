using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class WitchBehaviour : CharacterBehaviour
{
	public WitchBehaviour()
	{
		base.priority = 18;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.homeSettlement == null && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !character.currentRegion.IsRegionVillageCapacityReached() && character.faction != null && character.faction.factionType.type == FACTION_TYPE.Wiccans && character.characterClass.className == "Great Witch")
		{
			VillageSpot firstUnoccupiedVillageSpotThatCanAccomodateFaction = character.currentRegion.GetFirstUnoccupiedVillageSpotThatCanAccomodateFaction(character.faction.factionType.type);
			if (firstUnoccupiedVillageSpotThatCanAccomodateFaction != null)
			{
				Area coreSpot = firstUnoccupiedVillageSpotThatCanAccomodateFaction.coreSpot;
				StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, character.faction.factionType.mainResource);
				GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(character.faction.factionType.type, structureSetting));
				if (LandmarkManager.Instance.HasEnoughSpaceForStructure(randomElement.name, coreSpot.gridTileComponent.centerGridTile))
				{
					return character.jobComponent.TriggerFindNewVillage(coreSpot.gridTileComponent.centerGridTile, out producedJob, randomElement.name);
				}
			}
		}
		int num = 0;
		if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Free_Time)
		{
			num = ChanceData.GetChance(CHANCE_TYPE.Free_Time_Cultist_Do_Cult_Action);
		}
		else if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Work)
		{
			num = ChanceData.GetChance(CHANCE_TYPE.Work_Time_Cultist_Do_Cult_Action);
		}
		if (Random.Range(0, 100) < num)
		{
			return TryCreateCultistJob(character, ref log, out producedJob);
		}
		producedJob = null;
		return false;
	}

	private bool TryCreateCultistJob(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Cultist_Preach) && character.jobComponent.TryGetValidEvangelizeTarget(out var targetCharacter, RELIGION.Nature_Worship))
		{
			log = log + "\n" + character.name + " has cultist kit available and could not sabotage neighbour. Will create evangelize job.";
			return character.jobComponent.TryCreateEvangelizeJob(targetCharacter, out producedJob);
		}
		if (character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.HALLOWED_GROUND) is Inner_Maps.Location_Structures.HallowedGround hallowedGround && ChanceData.RollChance(CHANCE_TYPE.Cultist_Pilgrimage) && hallowedGround.IsNearHallowedGroundForPilgrimage(character) && character.jobComponent.GetNumOfTimesActionDone(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PILGRIMAGE]) < 3 && hallowedGround.claimedByReligion == character.religionComponent.religion)
		{
			return character.jobComponent.TryCreatePilgrimageJob(hallowedGround, out producedJob);
		}
		producedJob = null;
		return false;
	}
}
