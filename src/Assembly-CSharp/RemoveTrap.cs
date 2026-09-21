public class RemoveTrap : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public RemoveTrap()
		: base(INTERACTION_TYPE.REMOVE_TRAP)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Booby Trapped", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Snare Trapped", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Freezing Trapped", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Landmined", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
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
		goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Booby Trapped");
		if (goapNode.poiTarget is GenericTileObject genericTileObject)
		{
			if (genericTileObject.gridTileLocation.tileObjectComponent.hasSnareTrap)
			{
				genericTileObject.gridTileLocation.tileObjectComponent.SetHasSnareTrap(state: false, isPlayerSource: false);
			}
			if (genericTileObject.gridTileLocation.tileObjectComponent.hasFreezingTrap)
			{
				genericTileObject.gridTileLocation.tileObjectComponent.SetHasFreezingTrap(false, false);
			}
			if (genericTileObject.gridTileLocation.tileObjectComponent.hasLandmine)
			{
				genericTileObject.gridTileLocation.tileObjectComponent.SetHasLandmine(state: false);
			}
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
			if (!poiTarget.traitContainer.HasTrait("Booby Trapped") && !poiTarget.traitContainer.HasTrait("Snare Trapped") && !poiTarget.traitContainer.HasTrait("Freezing Trapped"))
			{
				return poiTarget.traitContainer.HasTrait("Landmined");
			}
			return true;
		}
		return false;
	}
}
