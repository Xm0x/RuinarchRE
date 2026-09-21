public class Impregnate : GoapAction
{
	public Impregnate()
		: base(INTERACTION_TYPE.IMPREGNATE)
	{
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Impregnate Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor != poiTarget)
			{
				return !(poiTarget as Character).isDead;
			}
			return false;
		}
		return false;
	}

	public void AfterImpregnateSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character)
		{
			character.traitContainer.AddTrait(character, "Impregnated");
		}
	}
}
