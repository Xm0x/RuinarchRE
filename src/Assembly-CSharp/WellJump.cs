public class WellJump : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public WellJump()
		: base(INTERACTION_TYPE.WELL_JUMP)
	{
		base.actionIconString = GoapActionStateDB.Death_Icon;
		base.logTags = new LOG_TAG[1];
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Well Jump Success", goapNode);
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		if (node.otherData != null && node.otherData.Length == 1)
		{
			string key = (string)node.otherData[0].obj;
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", key);
			log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
		}
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.IsAvailable())
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void AfterWellJumpSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.Death("normal", goapNode, null, goapNode.descriptionLog);
	}
}
