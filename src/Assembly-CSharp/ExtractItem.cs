using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class ExtractItem : GoapAction
{
	public ExtractItem()
		: base(INTERACTION_TYPE.EXTRACT_ITEM)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
	}

	protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden)
	{
		List<GoapEffect> list = RuinarchListPool<GoapEffect>.Claim(4);
		AddBaseExpectedEffectsToList(list);
		if (target.traitContainer.HasTrait("Wet") && target is TileObject)
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Water Flask", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		}
		if (target.traitContainer.HasTrait("Burning"))
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Ember", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		}
		if (target.traitContainer.HasTrait("Frozen") || target is SnowMound)
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Ice", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		}
		isOverridden = true;
		return list;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Extract Success", goapNode);
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		_ = node.poiTarget;
		IPointOfInterest poiTarget = node.poiTarget;
		string text = string.Empty;
		if (poiTarget.traitContainer.HasTrait("Wet"))
		{
			text += "Water Flask";
		}
		if (poiTarget.traitContainer.HasTrait("Burning"))
		{
			if (text != string.Empty)
			{
				text += ", ";
			}
			text += "Ember";
		}
		if (poiTarget is SnowMound || poiTarget.traitContainer.HasTrait("Frozen"))
		{
			if (text != string.Empty)
			{
				text += ", ";
			}
			text += "Ice";
		}
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", text);
		text = Utilities.GetArticleForWord(localizedValue) + " " + localizedValue;
		log.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		BaseSettlement settlement = null;
		if (target is TileObject && target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out settlement))
		{
			Faction owner = settlement.owner;
			if (actor.faction != null && owner != null && actor.faction.IsHostileWith(owner))
			{
				return 2000;
			}
		}
		return 250;
	}

	public void PreExtractSuccess(ActualGoapNode goapNode)
	{
		IPointOfInterest poiTarget = goapNode.poiTarget;
		string text = string.Empty;
		if (poiTarget.traitContainer.HasTrait("Wet"))
		{
			text += "Water Flask";
		}
		if (poiTarget.traitContainer.HasTrait("Burning"))
		{
			if (text != string.Empty)
			{
				text += ", ";
			}
			text += "Ember";
		}
		if (poiTarget is SnowMound || poiTarget.traitContainer.HasTrait("Frozen"))
		{
			if (text != string.Empty)
			{
				text += ", ";
			}
			text += "Ice";
		}
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", text);
		text = Utilities.GetArticleForWord(localizedValue) + " " + localizedValue;
		goapNode.descriptionLog.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
		goapNode.thoughtBubbleLog?.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
		goapNode.thoughtBubbleMovingLog?.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
	}

	public void AfterExtractSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		IPointOfInterest poiTarget = goapNode.poiTarget;
		if (poiTarget.traitContainer.HasTrait("Wet"))
		{
			Poisoned traitOrStatus = poiTarget.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
			int stacks = poiTarget.traitContainer.GetStacks("Poisoned");
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK);
			if (goapNode.associatedJobType == JOB_TYPE.DOUSE_FIRE || goapNode.associatedJobType == JOB_TYPE.DOUSE_FIRE_SELF)
			{
				TileObject tileObject2 = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK);
				TileObject tileObject3 = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK);
				if (traitOrStatus != null)
				{
					for (int i = 0; i < stacks; i++)
					{
						tileObject.traitContainer.AddTrait(tileObject, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
						tileObject2.traitContainer.AddTrait(tileObject2, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
						tileObject3.traitContainer.AddTrait(tileObject3, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
					}
				}
				actor.ObtainItem(tileObject);
				actor.ObtainItem(tileObject2);
				actor.ObtainItem(tileObject3);
			}
			else
			{
				if (traitOrStatus != null)
				{
					for (int j = 0; j < stacks; j++)
					{
						tileObject.traitContainer.AddTrait(tileObject, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
					}
				}
				actor.ObtainItem(tileObject);
			}
		}
		if (poiTarget.traitContainer.HasTrait("Burning"))
		{
			actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.EMBER));
		}
		if (poiTarget is SnowMound || poiTarget.traitContainer.HasTrait("Frozen"))
		{
			actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ICE));
		}
	}

	private bool HasHerbPlant(Character actor, IPointOfInterest poiTarget, object[] otherData)
	{
		return actor.HasItem("Herb Plant");
	}

	private bool HasWaterFlask(Character actor, IPointOfInterest poiTarget, object[] otherData)
	{
		return actor.HasItem("Water Flask");
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor != poiTarget && poiTarget.gridTileLocation != null)
			{
				if (!poiTarget.traitContainer.HasTrait("Wet", "Burning", "Frozen"))
				{
					return poiTarget is SnowMound;
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
