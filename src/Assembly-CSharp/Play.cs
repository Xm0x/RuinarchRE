using UtilityScripts;

public class Play : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Play()
		: base(INTERACTION_TYPE.PLAY)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
		validTimeOfDays = new TIME_IN_WORDS[4]
		{
			TIME_IN_WORDS.MORNING,
			TIME_IN_WORDS.LUNCH_TIME,
			TIME_IN_WORDS.AFTERNOON,
			TIME_IN_WORDS.EARLY_NIGHT
		};
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Play Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(6, 16);
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	public void PerTickPlaySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(6f);
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
			return actor == poiTarget;
		}
		return false;
	}
}
