public class CarryPatient : GoapAction
{
	public CarryPatient()
		: base(INTERACTION_TYPE.CARRY_PATIENT)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Social
		};
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.CARRIED_PATIENT, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Carry Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			Character character = poiTarget as Character;
			if (character.combatComponent.isInCombat || (character.interruptComponent.isInterrupted && character.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Cowering))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_unavailable";
			}
		}
		return goapActionInvalidity;
	}

	public void PreCarrySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.CarryPOI(goapNode.poiTarget);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			Character character = poiTarget as Character;
			if (character == actor)
			{
				return false;
			}
			if (character.stateComponent.currentState is CombatState)
			{
				return false;
			}
			if (character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld || character.currentRegion != actor.currentRegion)
			{
				return false;
			}
			if (actor.homeSettlement == null)
			{
				return false;
			}
			if (!actor.homeSettlement.HasStructure(STRUCTURE_TYPE.HOSPICE))
			{
				return false;
			}
			return character.carryComponent.IsNotBeingCarried();
		}
		return false;
	}
}
