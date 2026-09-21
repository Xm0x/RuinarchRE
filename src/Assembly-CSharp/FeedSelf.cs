using Traits;
using UtilityScripts;

public class FeedSelf : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public FeedSelf()
		: base(INTERACTION_TYPE.FEED_SELF)
	{
		base.actionIconString = GoapActionStateDB.Eat_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Feed Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		poiTarget.traitContainer.RemoveTrait(poiTarget, "Eating");
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && !(poiTarget as Character).carryComponent.IsNotBeingCarried())
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "target_carried";
		}
		return goapActionInvalidity;
	}

	public void PreFeedSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character)
		{
			character.traitContainer.AddTrait(character, "Eating");
		}
	}

	public void PerTickFeedSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character)
		{
			_ = goapNode.actor;
			if (character.needsComponent.HasNeeds())
			{
				character.needsComponent.AdjustFullness(20f, 0.15f);
			}
		}
	}

	public void AfterFeedSuccess(ActualGoapNode goapNode)
	{
		if (!(goapNode.poiTarget is Character character))
		{
			return;
		}
		Character actor = goapNode.actor;
		character.traitContainer.RemoveTrait(character, "Eating");
		if (!character.traitContainer.HasTrait("Vampire"))
		{
			return;
		}
		if (!actor.race.IsSapient() || (actor.traitContainer.HasTrait("Vampire") && !character.traitContainer.HasTrait("Cannibal")))
		{
			character.traitContainer.AddTrait(character, "Poor Meal", actor);
		}
		if (GameUtilities.RollChance(98))
		{
			actor.traitContainer.AddTrait(actor, "Lethargic", character);
			actor.traitContainer.GetTraitOrStatus<Trait>("Lethargic")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
			return;
		}
		if (!character.classComponent.IsStalkerCannotBeTurned() && actor.traitContainer.AddTrait(actor, "Vampire", character))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " contracted", LOG_TAG.Life_Changes, goapNode);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(actor, log, releaseLogAfter: true);
		}
		if (actor.isNormalCharacter)
		{
			character.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AdjustNumOfConvertedVillagers(1);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget.gridTileLocation != null && actor != poiTarget)
		{
			return true;
		}
		return false;
	}
}
