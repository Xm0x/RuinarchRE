public class EatInventoryItem : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

	public EatInventoryItem()
		: base(INTERACTION_TYPE.EAT_INVENTORY_ITEM)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Eat_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
		base.canBePerformedEvenIfPathImpossible = true;
		base.canBePerformedEvenIfTargetHasNoTileLocation = true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Eat Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 100;
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		base.OnActionStarted(node);
		LunchPack item = node.actor.GetItem<LunchPack>();
		if (item != null)
		{
			node.actor.ShowItemVisualCarryingPOI(item);
		}
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		node.actor.UncarryPOI(bringBackToInventory: true);
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		node.actor.UncarryPOI(bringBackToInventory: true);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.isBeingCarriedBy == actor && !actor.traitContainer.HasTrait("Paralyzed"))
			{
				return !actor.traitContainer.HasTrait("Vampire");
			}
			return false;
		}
		return false;
	}

	public void PerTickEatSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.needsComponent.HasNeeds())
		{
			goapNode.actor.needsComponent.AdjustFullness(25f, 0.2f);
		}
	}

	public void AfterEatSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.UncarryPOI(bringBackToInventory: true);
		goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.LUNCH_PACK);
	}
}
