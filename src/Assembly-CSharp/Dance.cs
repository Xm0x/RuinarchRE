using UtilityScripts;

public class Dance : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Dance()
		: base(INTERACTION_TYPE.DANCE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		validTimeOfDays = new TIME_IN_WORDS[3]
		{
			TIME_IN_WORDS.LUNCH_TIME,
			TIME_IN_WORDS.AFTERNOON,
			TIME_IN_WORDS.EARLY_NIGHT
		};
		base.actionIconString = GoapActionStateDB.Party_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Dance Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = Utilities.Rng.Next(90, 131);
		int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
		if (numOfTimesActionDone > 5)
		{
			num += 2000;
		}
		int num2 = 10 * numOfTimesActionDone;
		num += num2;
		Character[] array = actor.marker.inVisionCharacters.ToArray();
		foreach (Character character in array)
		{
			if (actor.relationshipContainer.IsFriendsWith(character) && character.currentActionNode != null && character.currentActionNode.action != null && (character.currentActionNode.action.goapType == INTERACTION_TYPE.SING || character.currentActionNode.action.goapType == INTERACTION_TYPE.PLAY_GUITAR))
			{
				num -= 35;
				break;
			}
		}
		return num;
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
			if (actor == poiTarget)
			{
				return actor.moodComponent.moodState == MOOD_STATE.Normal;
			}
			return false;
		}
		return false;
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	public void PreDanceSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
	}

	public void PerTickDanceSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(6f);
	}
}
