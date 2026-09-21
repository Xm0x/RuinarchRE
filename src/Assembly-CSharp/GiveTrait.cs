using Traits;

public class GiveTrait : GoapAction
{
	public GiveTrait()
		: base(INTERACTION_TYPE.GIVE_TRAIT)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Give Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			if (node.actor is Mothman mothman)
			{
				if (string.IsNullOrEmpty(mothman.stolenTraitName))
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.reason = "no_trait_give";
				}
			}
			else if (!node.actor.traitContainer.HasAnyNotHiddenTrait())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "no_trait_give";
			}
		}
		return goapActionInvalidity;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string empty = string.Empty;
		empty = ((!(node.actor is Mothman mothman)) ? ((string)node.otherData[0].obj) : mothman.stolenTraitName);
		log.AddToFillers(null, TraitManager.Instance.GetLocalizedNameOfTrait(empty), LOG_IDENTIFIER.STRING_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor.gridTileLocation != null;
		}
		return false;
	}

	public void AfterGiveSuccess(ActualGoapNode goapNode)
	{
		IPointOfInterest poiTarget = goapNode.poiTarget;
		Character actor = goapNode.actor;
		if (actor is Mothman mothman)
		{
			if (!string.IsNullOrEmpty(mothman.stolenTraitName) && !poiTarget.traitContainer.HasTrait(mothman.stolenTraitName))
			{
				poiTarget.traitContainer.AddTrait(poiTarget, mothman.stolenTraitName);
			}
			mothman.SetStolenTraitName(string.Empty);
			return;
		}
		Trait randomNonHiddenTrait = actor.traitContainer.GetRandomNonHiddenTrait();
		if (randomNonHiddenTrait != null)
		{
			actor.traitContainer.RemoveTrait(actor, randomNonHiddenTrait);
			if (!poiTarget.traitContainer.HasTrait(randomNonHiddenTrait.name))
			{
				poiTarget.traitContainer.AddTrait(poiTarget, randomNonHiddenTrait.name);
			}
		}
	}
}
