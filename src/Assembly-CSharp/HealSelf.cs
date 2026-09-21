using System;
using Goap.Unique_Action_Data;
using UnityEngine;

public class HealSelf : GoapAction
{
	public override Type uniqueActionDataType => typeof(HealSelfUAD);

	public HealSelf()
		: base(INTERACTION_TYPE.HEAL_SELF)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Cure_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Healing Potion", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasItemInInventory);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Heal Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void PreHealSuccess(ActualGoapNode goapNode)
	{
		TileObject item = goapNode.actor.GetItem(TILE_OBJECT_TYPE.HEALING_POTION);
		if (item != null && item.traitContainer.HasTrait("Poisoned"))
		{
			goapNode.GetConvertedUniqueActionData<HealSelfUAD>().SetUsedPoisonedHealingPotion(state: true);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", goapNode.action.goapName + " used_poison", base.logTags);
			log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			goapNode.OverrideDescriptionLog(log);
		}
	}

	public void PerTickHealSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.GetConvertedUniqueActionData<HealSelfUAD>().usedPoisonedHealingPotion)
		{
			goapNode.actor.AdjustHP(-100, ELEMENTAL_TYPE.Normal, triggerDeath: true);
		}
		else
		{
			goapNode.actor.AdjustHP(Mathf.FloorToInt((float)goapNode.actor.maxHP * 0.25f), ELEMENTAL_TYPE.Normal);
		}
	}

	public void AfterHealSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.GetConvertedUniqueActionData<HealSelfUAD>().usedPoisonedHealingPotion)
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
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
			goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);
		}
	}

	private bool HasItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.HasItem(TILE_OBJECT_TYPE.HEALING_POTION);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget == actor)
			{
				return !actor.traitContainer.HasTrait("Paralyzed");
			}
			return false;
		}
		return false;
	}
}
