using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class BloodSacrifice : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public BloodSacrifice()
		: base(INTERACTION_TYPE.BLOOD_SACRIFICE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
		base.actionIconString = GoapActionStateDB.Cult_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		Character character = node.target as Character;
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length >= 1 && otherData[0].obj is TileObject tileObject)
		{
			return tileObject.structureLocation;
		}
		return character.currentStructure;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		Character character = goapNode.target as Character;
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length >= 1 && otherData[0].obj is TileObject tileObject)
		{
			return tileObject.gridTileLocation;
		}
		return character.gridTileLocation;
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Carry Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCarried);
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

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		if (node.poiTarget is Character poi)
		{
			actor.UncarryPOI(poi, bringBackToInventory: false, addToLocation: false);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = false;
		invalidity.stateName = stateName;
		invalidity.reason = string.Empty;
		return invalidity;
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (target is Character target2)
		{
			if (witness.traitContainer.HasTrait("Psychopath", "Hemophiliac"))
			{
				reactions.Add(EMOTION.Arousal);
			}
			else if (witness.relationshipContainer.GetOpinionLabel(target2) == "Close Friend")
			{
				reactions.Add(EMOTION.Sadness);
			}
			else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor))
			{
				reactions.Add(EMOTION.Despair);
			}
		}
	}

	public void PreSacrificeSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		_ = goapNode.poiTarget;
		OtherData[] otherData = goapNode.otherData;
		LocationGridTile locationGridTile = null;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is TileObject tileObject)
		{
			locationGridTile = tileObject.gridTileLocation;
		}
		if (locationGridTile == null)
		{
			locationGridTile = actor.gridTileLocation;
		}
		goapNode.actor.UncarryPOI(goapNode.poiTarget, bringBackToInventory: false, addToLocation: true, locationGridTile);
	}

	public void AfterSacrificeSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		BaseSettlement homeSettlement = actor.homeSettlement;
		if (homeSettlement != null)
		{
			for (int i = 0; i < homeSettlement.residents.Count; i++)
			{
				Character character2 = homeSettlement.residents[i];
				if (!character2.isDead && character2 != character && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(homeSettlement))
				{
					character2.traitContainer.RemoveStatusAndStacks(character2, "Burnt");
					character2.traitContainer.RemoveStatusAndStacks(character2, "Injured");
					character2.traitContainer.RemoveStatusAndStacks(character2, "Frozen");
					character2.traitContainer.RemoveStatusAndStacks(character2, "Plagued");
					character2.traitContainer.RemoveStatusAndStacks(character2, "Poisoned");
					character2.traitContainer.AddTrait(character2, "Empowered", null, bypassElementalChance: false, GameManager.Instance.GetTicksBasedOnHour(48));
				}
			}
		}
		character.Death("normal", goapNode, null, goapNode.descriptionLog);
	}

	private bool IsCarried(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.IsPOICarriedOrInInventory(poiTarget);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is Character character)
		{
			if (character.isDead)
			{
				return false;
			}
			if (character.marker == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
