using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class DefaultWanderer : CharacterBehaviour
{
	public DefaultWanderer()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.needsComponent.isStarving)
		{
			if (character.needsComponent.PlanFullnessRecoveryActionsForFreeTime(out producedJob))
			{
				return true;
			}
		}
		else if (character.needsComponent.isHungry && GameUtilities.RollChance(20, ref log) && character.needsComponent.PlanFullnessRecoveryActionsForFreeTime(out producedJob))
		{
			return true;
		}
		if (character.needsComponent.isSulking)
		{
			if (CreateHappinessRecoveryJob(character, out producedJob))
			{
				return true;
			}
		}
		else if (character.needsComponent.isBored && GameUtilities.RollChance(20, ref log) && CreateHappinessRecoveryJob(character, out producedJob))
		{
			return true;
		}
		if (!character.HasTerritory() && character.currentRegion != null)
		{
			Area randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption = character.currentRegion.GetRandomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption(character);
			if (randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption != null)
			{
				character.SetTerritory(randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption);
			}
		}
		if (character.gridTileLocation != null)
		{
			if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory())
			{
				return character.jobComponent.TriggerRoamAroundTile(out producedJob);
			}
			if (character.isAtHomeStructure || character.IsInTerritory())
			{
				if (character.previousCharacterDataComponent.IsPreviousJobOrActionReturnHome())
				{
					TileObject unoccupiedBuiltTileObject = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
					if (unoccupiedBuiltTileObject != null)
					{
						character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, unoccupiedBuiltTileObject, out producedJob);
					}
					else
					{
						character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
					}
					return true;
				}
				TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
				if ((currentTimeInWordsOfTick == TIME_IN_WORDS.LUNCH_TIME || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTERNOON) && Random.Range(0, 100) < 25)
				{
					TileObject unoccupiedTileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
					if (unoccupiedTileObject != null && !character.traitContainer.HasTrait("Vampire") && character.limiterComponent.canDoTirednessRecovery)
					{
						character.PlanFixedJob(JOB_TYPE.IDLE_NAP, INTERACTION_TYPE.NAP, unoccupiedTileObject, out producedJob);
						return true;
					}
				}
				if ((currentTimeInWordsOfTick == TIME_IN_WORDS.MORNING || currentTimeInWordsOfTick == TIME_IN_WORDS.LUNCH_TIME || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTERNOON || currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT) && !character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea() && Random.Range(0, 100) < 25)
				{
					character.jobComponent.PlanIdleStrollOutside(out producedJob);
					return true;
				}
				if ((currentTimeInWordsOfTick == TIME_IN_WORDS.MORNING || currentTimeInWordsOfTick == TIME_IN_WORDS.LUNCH_TIME || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTERNOON) && Random.Range(0, 100) < 25 && !character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea())
				{
					WeightedDictionary<Character> characterToVisitWeights = GetCharacterToVisitWeights(character);
					if (characterToVisitWeights != null && characterToVisitWeights.GetTotalOfWeights() > 0)
					{
						Character character2 = characterToVisitWeights.PickRandomElementGivenWeights();
						LocationStructure homeStructure = character2.homeStructure;
						character.PlanFixedJob(JOB_TYPE.VISIT_FRIEND, INTERACTION_TYPE.VISIT, character2, out producedJob, new OtherData[2]
						{
							new LocationStructureOtherData(homeStructure),
							new CharacterOtherData(character2)
						});
						return true;
					}
				}
				TileObject unoccupiedBuiltTileObject2 = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
				if (unoccupiedBuiltTileObject2 != null)
				{
					character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, unoccupiedBuiltTileObject2, out producedJob);
					return true;
				}
				character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
				return true;
			}
			if ((float)character.currentHP < (float)character.maxHP * 0.5f)
			{
				if (character.homeStructure != null || character.HasTerritory())
				{
					character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
					return true;
				}
			}
			else
			{
				if (Random.Range(0, 100) < 50)
				{
					character.jobComponent.TriggerRoamAroundTile(out producedJob);
					return true;
				}
				if (character.homeStructure != null || character.HasTerritory())
				{
					character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
					return true;
				}
			}
		}
		return false;
	}

	private bool CreateHappinessRecoveryJob(Character p_character, out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), p_character, p_character);
		JobUtilities.PopulatePriorityLocationsForHappinessRecovery(p_character, goapPlanJob);
		goapPlanJob.SetDoNotRecalculate(state: true);
		producedJob = goapPlanJob;
		return true;
	}
}
