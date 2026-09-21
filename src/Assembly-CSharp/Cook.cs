using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class Cook : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Cook()
		: base(INTERACTION_TYPE.COOK)
	{
		base.actionIconString = GoapActionStateDB.Eat_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCarried);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Cook Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		Character character = node.poiTarget as Character;
		actor.UncarryPOI(character, bringBackToInventory: false, addToLocation: false);
		if (character != null && character.hasMarker)
		{
			character.marker.SetActiveState(state: true);
		}
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		Character character = node.poiTarget as Character;
		actor.UncarryPOI(character, bringBackToInventory: false, addToLocation: false);
		if (character != null && character.hasMarker)
		{
			character.marker.SetActiveState(state: true);
		}
	}

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		Character actor = node.actor;
		Character poi = node.poiTarget as Character;
		actor.UncarryPOI(poi, bringBackToInventory: false, addToLocation: false);
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		_ = node.actor;
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is TileObject tileObject)
		{
			if (tileObject.gridTileLocation != null)
			{
				return tileObject.structureLocation;
			}
			return null;
		}
		return base.GetTargetStructure(node);
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		if (goapNode.otherData != null && goapNode.otherData.Length == 1 && goapNode.otherData[0].obj is TileObject)
		{
			return goapNode.otherData[0].obj as TileObject;
		}
		return base.GetTargetToGoTo(goapNode);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = false;
		invalidity.stateName = stateName;
		invalidity.reason = string.Empty;
		IPointOfInterest poiTarget = node.poiTarget;
		if (!invalidity.isInvalid)
		{
			if (poiTarget is Character poi)
			{
				if (!node.actor.carryComponent.IsPOICarried(poi))
				{
					invalidity.isInvalid = true;
					invalidity.reason = "target_unavailable";
				}
			}
			else
			{
				invalidity.isInvalid = true;
				invalidity.reason = "target_unavailable";
			}
		}
		return invalidity;
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(target is Character character) || witness.traitContainer.HasTrait("Cannibal") || !character.race.IsSapient())
		{
			return;
		}
		reactions.Add(EMOTION.Shock);
		reactions.Add(EMOTION.Repulsed);
		if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(actor))
		{
			reactions.Add(EMOTION.Disappointment);
		}
		if (!witness.traitContainer.HasTrait("Psychopath"))
		{
			if (!witness.characterClass.IsCombatant())
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.relationshipContainer.IsEnemiesWith(character))
			{
				reactions.Add(EMOTION.Rage);
			}
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (target is Character character && (witness.relationshipContainer.IsFriendsWith(character) || witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character)) && !witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Despair);
			reactions.Add(EMOTION.Sadness);
		}
	}

	public void PreCookSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character)
		{
			goapNode.actor.UncarryPOI(bringBackToInventory: false, addToLocation: false);
			character.marker.SetActiveState(state: false);
		}
	}

	public void AfterCookSuccess(ActualGoapNode goapNode)
	{
		CharacterManager.Instance.CreateFoodPileForPOI(goapNode.poiTarget, goapNode.actor.gridTileLocation);
		if (!(goapNode.poiTarget is Character character))
		{
			return;
		}
		goapNode.actor.UncarryPOI(bringBackToInventory: false, addToLocation: false);
		if (character.isDead)
		{
			if (character.hasMarker)
			{
				character.DestroyMarker();
			}
		}
		else
		{
			character.SetDestroyMarkerOnDeath(state: true);
			character.Death("normal", goapNode, goapNode.actor, goapNode.descriptionLog, null, null, null, isPlayerSource: false, goapNode.actor);
		}
	}

	private bool IsCarried(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.IsPOICarriedOrInInventory(poiTarget);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			Character character = poiTarget as Character;
			if (character.isBeingCarriedBy != null && character.isBeingCarriedBy != actor)
			{
				return false;
			}
			if (otherData != null && otherData.Length == 1 && otherData[0].obj is TileObject { gridTileLocation: null })
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
