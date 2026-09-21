using System.Collections.Generic;

public class RemoveEnsnared : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public RemoveEnsnared()
		: base(INTERACTION_TYPE.REMOVE_ENSNARED)
	{
		base.actionIconString = GoapActionStateDB.Cure_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1];
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Ensnared", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && poiTarget.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.REMOVE_STATUS, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Ensnared", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "already_being_removed";
			goapActionInvalidity.shouldLogInvalidity = false;
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Remove Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target))
		{
			return 2000;
		}
		return 10;
	}

	public void AfterRemoveSuccess(ActualGoapNode goapNode)
	{
		goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Ensnared");
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null)
			{
				return false;
			}
			return poiTarget.traitContainer.HasTrait("Ensnared");
		}
		return false;
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		reactions.Add(EMOTION.Gratefulness);
	}
}
