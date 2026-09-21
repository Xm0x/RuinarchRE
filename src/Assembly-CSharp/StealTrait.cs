using Traits;

public class StealTrait : GoapAction
{
	public StealTrait()
		: base(INTERACTION_TYPE.STEAL_TRAIT)
	{
		base.actionIconString = GoapActionStateDB.Steal_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Steal Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && !node.poiTarget.traitContainer.HasAnyNotHiddenTrait())
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "no_trait";
		}
		return goapActionInvalidity;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor.gridTileLocation != null;
		}
		return false;
	}

	public void PreStealSuccess(ActualGoapNode goapNode)
	{
		IPointOfInterest poiTarget = goapNode.poiTarget;
		Character actor = goapNode.actor;
		Trait randomNonHiddenTrait = poiTarget.traitContainer.GetRandomNonHiddenTrait();
		if (randomNonHiddenTrait != null)
		{
			if (actor is Mothman mothman)
			{
				mothman.SetStolenTraitName(randomNonHiddenTrait.name);
			}
			else
			{
				actor.traitContainer.AddTrait(actor, randomNonHiddenTrait.name);
			}
			poiTarget.traitContainer.RemoveTrait(poiTarget, randomNonHiddenTrait);
			goapNode.descriptionLog.AddToFillers(null, TraitManager.Instance.GetLocalizedNameOfTrait(randomNonHiddenTrait.name), LOG_IDENTIFIER.STRING_1);
		}
	}
}
