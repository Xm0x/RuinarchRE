using UtilityScripts;

public class Tantrum : GoapAction
{
	private string reason;

	public Tantrum()
		: base(INTERACTION_TYPE.TANTRUM)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Anger_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Tantrum Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(3, 11);
	}

	public void PreTantrumSuccess(ActualGoapNode goapNode)
	{
		goapNode.descriptionLog.AddToFillers(null, (string)goapNode.otherData[0].obj, LOG_IDENTIFIER.STRING_1);
	}

	public void AfterTantrumSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Berserked");
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}
}
