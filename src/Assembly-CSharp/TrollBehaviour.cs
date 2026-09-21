using Traits;
using UnityEngine;
using UtilityScripts;

public class TrollBehaviour : BaseMonsterBehaviour
{
	public TrollBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.IsAtHome() && Random.Range(0, 100) < 10)
		{
			bool flag = false;
			if (character.homeSettlement != null)
			{
				flag = character.homeSettlement.HasTileObjectOfType(TILE_OBJECT_TYPE.TROLL_CAULDRON);
			}
			else if (character.homeStructure != null)
			{
				flag = character.homeStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.TROLL_CAULDRON);
			}
			if (!flag)
			{
				return character.jobComponent.TriggerBuildTrollCauldronJob(out producedJob);
			}
		}
		if (character.homeStructure != null && !character.isAtHomeStructure && !character.jobQueue.HasJob(JOB_TYPE.CAPTURE_CHARACTER) && (bool)character.marker)
		{
			Character character2 = null;
			Character character3 = null;
			for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
			{
				Character character4 = character.marker.inVisionCharacters[i];
				if (character4.isNormalCharacter && (!character4.limiterComponent.canPerform || !character4.limiterComponent.canMove))
				{
					if (character3 == null)
					{
						character3 = character4;
					}
					if (character4.traitContainer.HasTrait("Unconscious") && character4.traitContainer.GetTraitOrStatus<Unconscious>("Unconscious").IsResponsibleForTrait(character))
					{
						character2 = character4;
						break;
					}
				}
			}
			if (character2 == null)
			{
				character2 = character3;
			}
			if (character2 != null && character.jobComponent.TryTriggerCaptureCharacter(JOB_TYPE.CAPTURE_CHARACTER, character2, character.homeStructure, out producedJob, doNotRecalculate: true))
			{
				return true;
			}
		}
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if (currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT)
		{
			if (!character.isAtHomeStructure && !character.IsInHomeSettlement())
			{
				if (Random.Range(0, 100) < 30)
				{
					return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
				}
				Area area = null;
				area = ((character.homeSettlement == null) ? character.areaLocation.neighbourComponent.GetRandomAdjacentNoSettlementHextileWithinRegion() : character.homeSettlement.GetAPlainAdjacentArea());
				if (area != null)
				{
					return character.jobComponent.CreateGoToSpecificTileJob(area.gridTileComponent.GetRandomPassableTile(), out producedJob);
				}
				return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
			}
			Area area2 = null;
			area2 = ((character.homeSettlement == null) ? character.areaLocation.neighbourComponent.GetRandomAdjacentNoSettlementHextileWithinRegion() : character.homeSettlement.GetAPlainAdjacentArea());
			if (area2 != null)
			{
				return character.jobComponent.CreateGoToSpecificTileJob(area2.gridTileComponent.GetRandomPassableTile(), out producedJob);
			}
		}
		if (character.IsAtHome())
		{
			if (GameUtilities.RollChance(25))
			{
				FoodPile foodPile = null;
				if (character.homeSettlement != null)
				{
					foodPile = character.homeSettlement.GetFirstTileObjectOfType<FoodPile>(TILE_OBJECT_TYPE.HUMAN_MEAT, TILE_OBJECT_TYPE.ELF_MEAT, TILE_OBJECT_TYPE.ANIMAL_MEAT, TILE_OBJECT_TYPE.RAT_MEAT);
				}
				else if (character.homeStructure != null)
				{
					foodPile = character.homeStructure.GetFirstTileObjectOfType<FoodPile>(TILE_OBJECT_TYPE.HUMAN_MEAT, TILE_OBJECT_TYPE.ELF_MEAT, TILE_OBJECT_TYPE.ANIMAL_MEAT, TILE_OBJECT_TYPE.RAT_MEAT);
				}
				if (foodPile != null && character.jobComponent.CreateFullnessRecoveryOnSight(foodPile, cancelOtherFullnessRecoveryJobs: false, out producedJob))
				{
					return true;
				}
			}
			if (Random.Range(0, 100) < 35)
			{
				Character character5 = null;
				TrollCauldron trollCauldron = null;
				if (character.homeSettlement != null)
				{
					character5 = character.homeSettlement.GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(character);
					trollCauldron = character.homeSettlement.GetFirstTileObjectOfType<TrollCauldron>(TILE_OBJECT_TYPE.TROLL_CAULDRON);
				}
				else if (character.homeStructure != null)
				{
					character5 = character.homeStructure.GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(character);
					trollCauldron = character.homeStructure.GetFirstTileObjectOfType<TrollCauldron>(TILE_OBJECT_TYPE.TROLL_CAULDRON);
				}
				if (character5 != null && trollCauldron != null && character.jobComponent.TriggerCookJob(character5, trollCauldron, out producedJob))
				{
					return true;
				}
			}
			if (Random.Range(0, 100) < 10)
			{
				Character character6 = null;
				if (character.homeSettlement != null)
				{
					character6 = character.homeSettlement.GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(character);
				}
				else if (character.homeStructure != null)
				{
					character6 = character.homeStructure.GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(character);
				}
				if (character6 != null && character.jobComponent.CreateButcherJob(character6, JOB_TYPE.MONSTER_BUTCHER, out producedJob))
				{
					return true;
				}
			}
			return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
		}
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}
}
