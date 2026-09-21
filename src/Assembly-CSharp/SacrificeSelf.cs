public class SacrificeSelf : GoapAction
{
	public SacrificeSelf()
		: base(INTERACTION_TYPE.SACRIFICE_SELF)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Cult_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Player };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Sacrifice Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterSacrificeSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		GameManager.Instance.CreateParticleEffectAt(actor, PARTICLE_EFFECT.Sacrifice_Self, allowRotation: false);
		PlayerSkillManager.Instance.GetSkillData(actor.resonancePower).AdjustBonusCharges(1);
		actor.Death("normal", goapNode, null, goapNode.descriptionLog);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Character)
			{
				return poiTarget.traitContainer.HasTrait("Demon Cultist");
			}
			return false;
		}
		return false;
	}
}
