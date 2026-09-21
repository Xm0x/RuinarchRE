using UnityEngine;

public class Stumble : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Stumble()
		: base(INTERACTION_TYPE.STUMBLE)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.animationName = "Sleep";
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Stumble Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}

	public void PerTickStumbleSuccess(ActualGoapNode goapNode)
	{
		float num = (float)Random.Range(1, 6) / 100f;
		int num2 = Mathf.CeilToInt((float)goapNode.actor.maxHP * num);
		goapNode.actor.AdjustHP(-num2, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true);
	}

	public void AfterStumbleSuccess(ActualGoapNode goapNode)
	{
		if (!goapNode.actor.HasHealth())
		{
			goapNode.actor.Death("normal", goapNode);
		}
	}
}
