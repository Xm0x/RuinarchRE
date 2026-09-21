public class AbsorbLife : GoapAction
{
	public AbsorbLife()
		: base(INTERACTION_TYPE.ABSORB_LIFE)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsTargetDead);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Absorb Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Animal)
			{
				if (actor != poiTarget)
				{
					return poiTarget.mapObjectVisual;
				}
				return false;
			}
			if (poiTarget is Summon summon)
			{
				if (actor != poiTarget && (bool)poiTarget.mapObjectVisual)
				{
					return summon.isDead;
				}
				return false;
			}
		}
		return false;
	}

	private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is Character character)
		{
			return character.isDead;
		}
		return true;
	}

	public void AfterAbsorbSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.necromancerTrait.AdjustLifeAbsorbed(2);
		if (goapNode.poiTarget is Character character)
		{
			character.DestroyMarker();
		}
	}
}
