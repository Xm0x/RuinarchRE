public class AbsorbPower : GoapAction
{
	public AbsorbPower()
		: base(INTERACTION_TYPE.ABSORB_POWER)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Absorb Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is Character)
		{
			if (actor != poiTarget)
			{
				return poiTarget.mapObjectVisual;
			}
			return false;
		}
		return false;
	}

	public void AfterAbsorbSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		if (goapNode.actor.necromancerTrait != null)
		{
			goapNode.actor.necromancerTrait.AdjustEnergy(2);
		}
		else
		{
			goapNode.actor.combatComponent.AdjustIntelligenceModifier(2);
			goapNode.actor.piercingAndResistancesComponent.AdjustBasePiercing(5f);
			goapNode.actor.piercingAndResistancesComponent.AdjustAllResistances(5f);
		}
		if (character.hasMarker)
		{
			character.DestroyMarker();
		}
	}
}
