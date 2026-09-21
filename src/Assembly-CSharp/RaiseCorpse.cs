using UnityEngine;

public class RaiseCorpse : GoapAction
{
	public RaiseCorpse()
		: base(INTERACTION_TYPE.RAISE_CORPSE)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Work,
			LOG_TAG.Life_Changes
		};
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Raise Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
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

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		IPointOfInterest poiTarget = node.poiTarget;
		Character character = null;
		if (poiTarget is Character)
		{
			character = poiTarget as Character;
		}
		else if (poiTarget is Tombstone)
		{
			character = (poiTarget as Tombstone).character;
		}
		if (character != null)
		{
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			Character character = null;
			if (poiTarget is Character character2)
			{
				character = character2;
			}
			else if (poiTarget is Tombstone { gridTileLocation: not null } tombstone && (bool)tombstone.mapObjectVisual && tombstone.character != null)
			{
				character = tombstone.character;
			}
			if (character is Summon)
			{
				if (character.isDead && character.hasMarker)
				{
					return !character.hasBeenRaisedFromDead;
				}
				return false;
			}
			if (character != null && character.isDead)
			{
				return character.hasMarker;
			}
			return false;
		}
		return false;
	}

	public void AfterRaiseSuccess(ActualGoapNode goapNode)
	{
		IPointOfInterest poiTarget = goapNode.poiTarget;
		Character character = null;
		if (poiTarget is Character)
		{
			character = poiTarget as Character;
		}
		else if (poiTarget is Tombstone)
		{
			character = (poiTarget as Tombstone).character;
		}
		SUMMON_TYPE monsterType = SUMMON_TYPE.Skeleton;
		if (goapNode.actor is Ghoul)
		{
			monsterType = SUMMON_TYPE.Ghoul;
		}
		if (character != null && character.hasMarker)
		{
			Summon arg = CharacterManager.Instance.RaiseFromDeadReplaceCharacterWithMonsterType(monsterType, character, goapNode.actor.faction);
			if (goapNode.actor.classComponent.characterClass.className == "Necromancer")
			{
				Messenger.Broadcast(CharacterSignals.ON_CHARACTER_RAISE_DEAD_BY_NECRO, arg);
			}
		}
		else
		{
			Debug.LogWarning("Could not raise " + character?.name + " because it's marker is null!");
		}
	}
}
