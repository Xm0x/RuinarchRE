public class EatCorpse : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

	public EatCorpse()
		: base(INTERACTION_TYPE.EAT_CORPSE)
	{
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.actionIconString = GoapActionStateDB.Eat_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsTargetDead);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Eat Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is Character { numOfNonSecretActionsBeingPerformedOnThis: >0 })
		{
			goapActionInvalidity.isInvalid = true;
		}
		return goapActionInvalidity;
	}

	public override bool IsFullnessRecoveryAction()
	{
		return true;
	}

	private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is Character character)
		{
			return character.isDead;
		}
		return false;
	}

	public void PreEatSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.associatedJobType == JOB_TYPE.MONSTER_EAT_CORPSE || goapNode.associatedJobType == JOB_TYPE.HUNT_PREY)
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Abstain Fullness");
		}
	}

	public void PerTickEatSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.needsComponent.HasNeeds())
		{
			goapNode.actor.needsComponent.AdjustFullness(10f);
		}
	}

	public void AfterEatSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character { hasMarker: not false } character)
		{
			if (goapNode.actor.race == RACE.ELVES && (character.race == RACE.RAT || character.race == RACE.RATMAN))
			{
				goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poor Meal");
			}
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, goapNode.poiTarget, GoapPlanJob.Target_Already_Dead_Reason);
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, goapNode.poiTarget, GoapPlanJob.Target_Already_Dead_Reason);
			character.DestroyMarker();
		}
	}
}
