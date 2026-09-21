using Inner_Maps.Location_Structures;

public class ResolveCombat : GoapAction
{
	public ResolveCombat()
		: base(INTERACTION_TYPE.RESOLVE_COMBAT)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.doesNotStopTargetCharacter = true;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
		base.shouldAddLogs = false;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.STARTS_COMBAT, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCombatFinished);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		return node.actor.gridTileLocation.structure;
	}

	public override void Perform(ActualGoapNode actionNode)
	{
		base.Perform(actionNode);
		SetState("Combat Success", actionNode);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		bool isInvalid = false;
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		return invalidity;
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 50;
	}

	private bool IsCombatFinished(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType)
	{
		if (target is Character character)
		{
			if (jobType.IsJobLethal())
			{
				if (character.isDead)
				{
					return true;
				}
			}
			else if (character.traitContainer.HasTrait("Unconscious", "Stoned") || character.isDead)
			{
				return true;
			}
			return false;
		}
		return target.gridTileLocation == null;
	}
}
