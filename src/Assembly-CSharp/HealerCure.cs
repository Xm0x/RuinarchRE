using System;
using System.Collections.Generic;
using Goap.Unique_Action_Data;

public class HealerCure : GoapAction
{
	public override Type uniqueActionDataType => typeof(CureCharacterUAD);

	public HealerCure()
		: base(INTERACTION_TYPE.HEALER_CURE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Cure_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Healer Cure Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterHealerCureSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.moneyComponent.AdjustCoins(33);
		if (goapNode.actor.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) >= 3)
		{
			Level3Effect(goapNode);
		}
		else if (goapNode.actor.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) >= 2)
		{
			Level2Effect(goapNode);
		}
		goapNode.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(10, goapNode.actor);
	}

	private void Level2Effect(ActualGoapNode node)
	{
		if (node.target.traitContainer.HasTrait("Injured"))
		{
			node.target.traitContainer.RemoveStatusAndStacks(node.target, "Injured");
		}
		if (node.target.traitContainer.HasTrait("Poison"))
		{
			node.target.traitContainer.RemoveStatusAndStacks(node.target, "Poison");
		}
		node.target.traitContainer.RemoveStatusAndStacks(node.target, "Burnt");
	}

	private void Level3Effect(ActualGoapNode node)
	{
		Level2Effect(node);
		if (node.target.traitContainer.HasTrait("Plagued"))
		{
			node.target.traitContainer.RemoveStatusAndStacks(node.target, "Plagued");
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return poiTarget is Character;
		}
		return false;
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		reactions.Add(EMOTION.Gratefulness);
	}
}
