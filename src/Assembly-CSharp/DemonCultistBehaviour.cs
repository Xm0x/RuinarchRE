using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class DemonCultistBehaviour : CharacterBehaviour
{
	public DemonCultistBehaviour()
	{
		base.priority = 18;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.homeSettlement == null && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !character.currentRegion.IsRegionVillageCapacityReached() && character.faction != null && character.faction.factionType.type == FACTION_TYPE.Demon_Cult && character.characterClass.className == "Demon Cult Leader")
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
			num = ((!character.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT)) ? ChanceData.GetChance(CHANCE_TYPE.Free_Time_Cultist_Do_Cult_Action) : 10);
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

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeCultist();
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.behaviourComponent.OnNoLongerCultist();
	}

	public override void OnLoadBehaviourToCharacter(Character character)
	{
		base.OnLoadBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeCultist();
	}

	public bool TryCreateCultistJob(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (!character.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT) && character.homeStructure?.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT) == null)
		{
			log = log + "\n" + character.name + " has no cultist kit available. Will create obtain personal item job.";
			bool flag = character.jobComponent.TryCreateObtainPersonalItemJob("Cultist Kit", out producedJob);
			if (flag)
			{
				GoapPlanJob goapPlanJob = producedJob as GoapPlanJob;
				if (character.homeSettlement != null)
				{
					JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(character, goapPlanJob, INTERACTION_TYPE.NONE);
					if (character.homeStructure != null)
					{
						goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.NONE, character.homeStructure);
					}
					List<LocationStructure> structuresOfType = character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
					if (structuresOfType != null)
					{
						for (int i = 0; i < structuresOfType.Count; i++)
						{
							LocationStructure location = structuresOfType[i];
							goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.NONE, location);
						}
					}
					List<LocationStructure> structuresOfType2 = character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
					if (structuresOfType2 != null)
					{
						for (int j = 0; j < structuresOfType2.Count; j++)
						{
							LocationStructure location2 = structuresOfType2[j];
							goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.NONE, location2);
						}
					}
				}
				producedJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).mainRecipe.ingredient.amount });
			}
			return flag;
		}
		if (GameUtilities.RollChance(30) && character.jobComponent.TryGetValidSabotageNeighbourTarget(out var targetCharacter))
		{
			log = log + "\n" + character.name + " has cultist kit available. Will create sabotage neighbour job.";
			return character.jobComponent.TryCreateSabotageNeighbourJob(targetCharacter, out producedJob);
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Demon_Cultist_Preach) && character.jobComponent.TryGetValidEvangelizeTarget(out targetCharacter, RELIGION.Demon_Worship))
		{
			log = log + "\n" + character.name + " has cultist kit available and could not sabotage neighbour. Will create evangelize job.";
			return character.jobComponent.TryCreateEvangelizeJob(targetCharacter, out producedJob);
		}
		if (character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.HALLOWED_GROUND) is Inner_Maps.Location_Structures.HallowedGround hallowedGround && ChanceData.RollChance(CHANCE_TYPE.Cultist_Pilgrimage) && hallowedGround.IsNearHallowedGroundForPilgrimage(character) && character.jobComponent.GetNumOfTimesActionDone(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PILGRIMAGE]) < 3 && hallowedGround.claimedByReligion == character.religionComponent.religion)
		{
			return character.jobComponent.TryCreatePilgrimageJob(hallowedGround, out producedJob);
		}
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BRAINWASH);
		if (skillData.hasUnliChaosOrbs || skillData.hasRemainingChaosOrbs)
		{
			return character.jobComponent.TryCreateDarkRitualJob(out producedJob);
		}
		producedJob = null;
		return false;
	}
}
