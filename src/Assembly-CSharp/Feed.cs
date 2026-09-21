using System;
using System.Collections.Generic;
using Goap.Unique_Action_Data;
using UnityEngine;

public class Feed : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public override Type uniqueActionDataType => typeof(FeedUAD);

	public Feed()
		: base(INTERACTION_TYPE.FEED)
	{
		base.actionIconString = GoapActionStateDB.FirstAid_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FEED, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), ActorHasFood);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
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

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		actor.UncarryPOI();
		poiTarget.traitContainer.RemoveTrait(poiTarget, "Eating");
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		actor.UncarryPOI();
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

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(target is Character target2))
		{
			return;
		}
		FeedUAD convertedUniqueActionData = node.GetConvertedUniqueActionData<FeedUAD>();
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(target2);
		if (convertedUniqueActionData.usedPoisonedFood)
		{
			reactions.Add(EMOTION.Shock);
			if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
			{
				reactions.Add(EMOTION.Anger);
			}
			return;
		}
		switch (opinionLabel)
		{
		case "Friend":
		case "Close Friend":
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Gratefulness);
			}
			break;
		case "Rival":
			reactions.Add(EMOTION.Disapproval);
			break;
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (!(target is Character character))
		{
			return;
		}
		if (node.GetConvertedUniqueActionData<FeedUAD>().usedPoisonedFood)
		{
			reactions.Add(EMOTION.Shock);
			reactions.Add(EMOTION.Betrayal);
		}
		else if (character.traitContainer.HasTrait("Vampire"))
		{
			reactions.Add(EMOTION.Resentment);
		}
		else
		{
			if (character.traitContainer.HasTrait("Psychopath"))
			{
				return;
			}
			if (character.relationshipContainer.IsEnemiesWith(actor))
			{
				if (UnityEngine.Random.Range(0, 100) < 30)
				{
					reactions.Add(EMOTION.Gratefulness);
				}
				if (UnityEngine.Random.Range(0, 100) < 20)
				{
					reactions.Add(EMOTION.Embarassment);
				}
			}
			else
			{
				reactions.Add(EMOTION.Gratefulness);
			}
		}
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		base.OnActionStarted(node);
		for (int i = 0; i < node.actor.items.Count; i++)
		{
			TileObject tileObject = node.actor.items[i];
			if (tileObject.resourceStorageComponent.HasResourceAmount(RESOURCE.FOOD, 10))
			{
				node.actor.ShowItemVisualCarryingPOI(tileObject);
				break;
			}
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.GetConvertedUniqueActionData<FeedUAD>().usedPoisonedFood)
		{
			return REACTABLE_EFFECT.Negative;
		}
		return REACTABLE_EFFECT.Positive;
	}

	public void PreFeedSuccess(ActualGoapNode goapNode)
	{
		if (!(goapNode.poiTarget is Character character))
		{
			return;
		}
		if (character.traitContainer.HasTrait("Vampire"))
		{
			character.traitContainer.AddTrait(character, "Abstain Fullness");
		}
		character.traitContainer.AddTrait(character, "Eating");
		if (goapNode.actor.carryComponent.carriedPOI is ResourcePile resourcePile)
		{
			FeedUAD convertedUniqueActionData = goapNode.GetConvertedUniqueActionData<FeedUAD>();
			if (resourcePile.traitContainer.HasTrait("Poisoned"))
			{
				convertedUniqueActionData.SetUsedPoisonedFood(state: true);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", goapNode.action.goapName + " used_poison", base.logTags);
				log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				goapNode.OverrideDescriptionLog(log);
			}
			resourcePile.AdjustResourceInPile(-20);
		}
	}

	public void PerTickFeedSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character)
		{
			if (goapNode.GetConvertedUniqueActionData<FeedUAD>().usedPoisonedFood)
			{
				character.AdjustHP(-100, ELEMENTAL_TYPE.Normal, triggerDeath: true);
			}
			if (!character.traitContainer.HasTrait("Vampire") && character.needsComponent.HasNeeds())
			{
				character.needsComponent.AdjustFullness(10f, 0.08f);
			}
		}
	}

	public void AfterFeedSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character)
		{
			FeedUAD convertedUniqueActionData = goapNode.GetConvertedUniqueActionData<FeedUAD>();
			character.traitContainer.RemoveTrait(character, "Eating");
			if (goapNode.actor != character)
			{
				if (convertedUniqueActionData.usedPoisonedFood)
				{
					character.relationshipContainer.AdjustOpinion(character, goapNode.actor, "Poisoned_Me", -10);
				}
				else
				{
					character.relationshipContainer.AdjustOpinion(character, goapNode.actor, "Helped_Me", 5);
				}
			}
			if (character.traitContainer.HasTrait("Vampire"))
			{
				character.traitContainer.AddTrait(character, "Sick", goapNode.actor);
			}
			if (convertedUniqueActionData.usedPoisonedFood)
			{
				character.traitContainer.AddTrait(character, "Poisoned", goapNode.actor, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
			}
		}
		goapNode.actor.UncarryPOI();
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget.gridTileLocation != null && actor != poiTarget)
		{
			return true;
		}
		return false;
	}

	private bool ActorHasFood(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.FOOD, 10))
		{
			return true;
		}
		if (actor.items.Count > 0)
		{
			for (int i = 0; i < actor.items.Count; i++)
			{
				if (actor.items[i].resourceStorageComponent.HasResourceAmount(RESOURCE.FOOD, 10))
				{
					return true;
				}
			}
		}
		if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is FoodPile)
		{
			return true;
		}
		return false;
	}
}
