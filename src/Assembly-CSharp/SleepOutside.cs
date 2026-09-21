using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SleepOutside : GoapAction
{
	public SleepOutside()
		: base(INTERACTION_TYPE.SLEEP_OUTSIDE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
		base.actionIconString = GoapActionStateDB.Sleep_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Rest Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 160;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		actor.traitContainer.RemoveTrait(actor, "Resting");
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		if (node.actor.traitContainer.HasTrait("Paralyzed"))
		{
			return node.actor.currentStructure;
		}
		if (node.actor.homeStructure != null && node.actor.homeStructure.structureType == STRUCTURE_TYPE.DWELLING)
		{
			return node.actor.homeStructure;
		}
		return base.GetTargetStructure(node);
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		if (goapNode.actor.traitContainer.HasTrait("Paralyzed"))
		{
			return goapNode.actor.gridTileLocation;
		}
		if (goapNode.actor.homeStructure != null && goapNode.actor.homeStructure.structureType == STRUCTURE_TYPE.DWELLING)
		{
			if (goapNode.actor.isAtHomeStructure)
			{
				return goapNode.actor.gridTileLocation;
			}
			return null;
		}
		return goapNode.actor.gridTileLocation;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area))
			{
				return false;
			}
			return actor == poiTarget;
		}
		return false;
	}

	public void PreRestSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
	}

	public void PerTickRestSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		CharacterNeedsComponent needsComponent = actor.needsComponent;
		if (needsComponent.HasNeeds())
		{
			needsComponent.AdjustTiredness(0.25f);
			if (actor.race != RACE.RATMAN && !actor.partyComponent.isActiveMember)
			{
				needsComponent.AdjustHappiness(-0.2f);
			}
		}
	}

	public void AfterRestSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
	}
}
