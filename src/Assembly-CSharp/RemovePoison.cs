using System.Collections.Generic;

public class RemovePoison : GoapAction
{
	private Precondition _antidotePrecondition;

	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public RemovePoison()
		: base(INTERACTION_TYPE.REMOVE_POISON)
	{
		base.actionIconString = GoapActionStateDB.Cure_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1];
		_antidotePrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Antidote", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasAntidote);
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Poisoned", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		Precondition result = null;
		if (target is Character)
		{
			result = _antidotePrecondition;
		}
		isOverridden = true;
		return result;
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
		goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Poisoned");
		TileObject item = goapNode.actor.GetItem(TILE_OBJECT_TYPE.ANTIDOTE);
		if (item != null)
		{
			goapNode.actor.UnobtainItem(item);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null)
			{
				return false;
			}
			return poiTarget.traitContainer.HasTrait("Poisoned");
		}
		return false;
	}

	private bool HasAntidote(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.HasItem(TILE_OBJECT_TYPE.ANTIDOTE);
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		reactions.Add(EMOTION.Gratefulness);
	}
}
