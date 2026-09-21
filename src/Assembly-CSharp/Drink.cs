using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class Drink : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

	public Drink()
		: base(INTERACTION_TYPE.DRINK)
	{
		base.actionIconString = GoapActionStateDB.Drink_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Drink Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.traitContainer.HasTrait("Enslaved") && (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)))
		{
			return 2000;
		}
		BaseSettlement settlement = null;
		if (target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out settlement))
		{
			Faction owner = settlement.owner;
			if (actor.faction != null && owner != null && actor.faction.IsHostileWith(owner))
			{
				return 2000;
			}
		}
		if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.isActiveMember && target.gridTileLocation != null && actor.gridTileLocation != null)
		{
			LocationGridTile centerGridTile = target.gridTileLocation.area.gridTileComponent.centerGridTile;
			float distanceTo = actor.areaLocation.gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile);
			int num = InnerMapManager.AreaLocationGridTileSize.x * 3;
			if (distanceTo > (float)num)
			{
				return 2000;
			}
		}
		int num2 = Utilities.Rng.Next(80, 121);
		if (actor.traitContainer.HasTrait("Alcoholic"))
		{
			return num2 - 35;
		}
		if (job != null && (job.jobType == JOB_TYPE.OPINION_REDUCTION_REACTION || job.jobType == JOB_TYPE.TRIGGER_FLAW))
		{
			return num2 - 50;
		}
		int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if (currentTimeInWordsOfTick == TIME_IN_WORDS.MORNING || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTERNOON)
		{
			num2 += 2000;
		}
		if (numOfTimesActionDone > 5)
		{
			return num2 + 2000;
		}
		int num3 = 10 * numOfTimesActionDone;
		return num2 + num3;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (witness.traitContainer.HasTrait("Alcoholic"))
		{
			return REACTABLE_EFFECT.Positive;
		}
		return REACTABLE_EFFECT.Neutral;
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	public void PreDrinkSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
		LocationStructure locationStructure = goapNode.poiTarget.gridTileLocation?.structure;
		if (locationStructure != null && locationStructure.structureType == STRUCTURE_TYPE.TAVERN && locationStructure is ManMadeStructure manMadeStructure && manMadeStructure.HasAssignedWorker())
		{
			string id = manMadeStructure.assignedWorkerIDs[0];
			DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id).moneyComponent.AdjustCoins(33);
		}
	}

	public void PerTickDrinkSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(goapNode.actor.traitContainer.HasTrait("Alcoholic") ? 1.35f : 2f);
	}

	public void AfterDrinkSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Drunk");
		if ((goapNode.actor.moodComponent.moodState == MOOD_STATE.Bad && GameUtilities.RollChance(2)) || (goapNode.actor.moodComponent.moodState == MOOD_STATE.Critical && GameUtilities.RollChance(4)))
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Alcoholic");
		}
		goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Withdrawal");
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TAVERN && poiTarget.IsAvailable())
			{
				return !actor.traitContainer.HasTrait("Agoraphobic");
			}
			return false;
		}
		return false;
	}
}
