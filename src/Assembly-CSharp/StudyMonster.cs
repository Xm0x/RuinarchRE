public class StudyMonster : GoapAction
{
	public StudyMonster()
		: base(INTERACTION_TYPE.STUDY_MONSTER)
	{
		base.actionIconString = GoapActionStateDB.Inspect_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), HasUnconscious);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Study Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget != actor)
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	private bool HasUnconscious(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return (poiTarget as Character).traitContainer.HasTrait("Unconscious");
	}

	public void AfterStudySuccess(ActualGoapNode goapNode)
	{
	}
}
