using System;
using System.Collections.Generic;
using Goap.Unique_Action_Data;
using UnityEngine;

public class FirstAidCharacter : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public override Type uniqueActionDataType => typeof(FirstAidCharacterUAD);

	public FirstAidCharacter()
		: base(INTERACTION_TYPE.FIRST_AID_CHARACTER)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.FirstAid_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Healing Potion", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasHealingPotion);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Injured", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("First Aid Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target))
		{
			return 2000;
		}
		return 10;
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
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(target2);
		if (node.GetConvertedUniqueActionData<FirstAidCharacterUAD>().usedPoisonedHealingPotion)
		{
			reactions.Add(EMOTION.Shock);
			if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
			{
				reactions.Add(EMOTION.Anger);
			}
		}
		else
		{
			if (target == actor)
			{
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
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (!(target is Character character))
		{
			return;
		}
		if (node.GetConvertedUniqueActionData<FirstAidCharacterUAD>().usedPoisonedHealingPotion)
		{
			reactions.Add(EMOTION.Shock);
			reactions.Add(EMOTION.Betrayal);
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

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.poiTarget is Character otherCharacter)
		{
			FirstAidCharacterUAD convertedUniqueActionData = node.GetConvertedUniqueActionData<FirstAidCharacterUAD>();
			if (witness.IsHostileWith(otherCharacter) || convertedUniqueActionData.usedPoisonedHealingPotion)
			{
				return REACTABLE_EFFECT.Negative;
			}
		}
		return REACTABLE_EFFECT.Positive;
	}

	public void PreFirstAidSuccess(ActualGoapNode goapNode)
	{
		TileObject item = goapNode.actor.GetItem(TILE_OBJECT_TYPE.HEALING_POTION);
		if (item != null && item.traitContainer.HasTrait("Poisoned"))
		{
			goapNode.GetConvertedUniqueActionData<FirstAidCharacterUAD>().SetUsedPoisonedHealingPotion(state: true);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", goapNode.action.goapName + " used_poison", base.logTags);
			log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			goapNode.OverrideDescriptionLog(log);
		}
	}

	public void AfterFirstAidSuccess(ActualGoapNode goapNode)
	{
		FirstAidCharacterUAD convertedUniqueActionData = goapNode.GetConvertedUniqueActionData<FirstAidCharacterUAD>();
		if (goapNode.poiTarget is Character character && goapNode.actor != character)
		{
			if (convertedUniqueActionData.usedPoisonedHealingPotion)
			{
				character.relationshipContainer.AdjustOpinion(character, goapNode.actor, "Poisoned_Me", -10);
			}
			else
			{
				character.relationshipContainer.AdjustOpinion(character, goapNode.actor, "Helped_Me", 5);
			}
		}
		if (convertedUniqueActionData.usedPoisonedHealingPotion)
		{
			goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Poisoned", goapNode.actor, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
			goapNode.poiTarget.AdjustHP(-300, ELEMENTAL_TYPE.Normal, triggerDeath: true, goapNode.actor);
			bool flag = false;
			for (int i = 0; i < goapNode.actor.items.Count; i++)
			{
				TileObject tileObject = goapNode.actor.items[i];
				if (tileObject.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION && tileObject.traitContainer.HasTrait("Poisoned"))
				{
					goapNode.actor.UnobtainItem(tileObject);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);
			}
		}
		else
		{
			goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Injured", goapNode.actor);
			goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);
		}
	}

	private bool HasHealingPotion(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.HasItem(TILE_OBJECT_TYPE.HEALING_POTION);
	}
}
