using Traits;

public class HaveAffair : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public HaveAffair()
		: base(INTERACTION_TYPE.HAVE_AFFAIR)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Flirt_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Social
		};
		base.shouldAddLogs = false;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Affair Success", goapNode);
	}

	public void AfterAffairSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, goapNode.poiTarget);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (!poiTarget.IsAvailable())
			{
				return false;
			}
			if (actor == poiTarget)
			{
				return false;
			}
			Character character = poiTarget as Character;
			Unfaithful traitOrStatus = actor.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful");
			if (traitOrStatus != null)
			{
				if (traitOrStatus.IsCompatibleBasedOnSexualityAndOpinions(actor, character) && RelationshipManager.Instance.GetValidator(actor).CanHaveRelationship(actor, character, RELATIONSHIP_TYPE.AFFAIR))
				{
					return true;
				}
			}
			else if (RelationshipManager.IsSexuallyCompatible(actor, character) && RelationshipManager.Instance.GetValidator(actor).CanHaveRelationship(actor, character, RELATIONSHIP_TYPE.AFFAIR))
			{
				return true;
			}
		}
		return false;
	}
}
