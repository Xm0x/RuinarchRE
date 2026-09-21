using System;
using Traits;
using UnityEngine;

[Obsolete("This is no longer advertised by anything.")]
public class Accident : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Accident()
		: base(INTERACTION_TYPE.ACCIDENT)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1];
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode actionNode)
	{
		base.Perform(actionNode);
		SetState("Accident Success", actionNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 5;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}

	public void AfterAccidentSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Injured");
		goapNode.actor.traitContainer.GetTraitOrStatus<Trait>("Injured")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
		float num = (float)UnityEngine.Random.Range(5, 26) / 100f;
		int num2 = Mathf.CeilToInt((float)goapNode.actor.maxHP * num);
		goapNode.actor.AdjustHP(-num2, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true);
		if (!goapNode.actor.HasHealth())
		{
			goapNode.actor.Death("normal", goapNode);
		}
	}
}
