using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Angry : Status
{
	private Character owner;

	private readonly List<Character> _responsibleCharactersStack;

	public List<Character> responsibleCharactersStack => _responsibleCharactersStack;

	public override Type serializedData => typeof(SaveDataAngry);

	public override bool shouldBeLoadedInMainThread => true;

	public Angry()
	{
		name = "Angry";
		description = "Something or someone has made it mad!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
		moodEffect = -3;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 0.5f;
		hindersSocials = true;
		_responsibleCharactersStack = new List<Character>();
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataAngry saveDataAngry = p_saveDataTrait as SaveDataAngry;
		if (saveDataAngry.characterIDs == null)
		{
			return;
		}
		for (int i = 0; i < saveDataAngry.characterIDs.Count; i++)
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(saveDataAngry.characterIDs[i]);
			if (characterByPersistentID != null)
			{
				_responsibleCharactersStack.Add(characterByPersistentID);
			}
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (!(addedTo is Character character))
		{
			return;
		}
		if (character.isDead)
		{
			Debug.LogWarning(GameManager.Instance.TodayLogString() + character.name + " is already dead but gained an angry status!");
		}
		owner = character;
		for (int i = 0; i < responsibleCharactersStack.Count; i++)
		{
			Character character2 = responsibleCharactersStack[i];
			if (character2 != null)
			{
				owner.relationshipContainer.AdjustOpinion(owner, character2, "Anger", -30, "", createJobsOnReduce: false);
			}
		}
		if (character.HasAfflictedByPlayerWith("Hothead"))
		{
			DispenseChaosOrbsForAffliction(character, PLAYER_SKILL_TYPE.HOTHEADED, 1);
		}
		character.marker.visionColliderComponent.VoteToUnFilterVision();
		Messenger.AddListener(Signals.HOUR_STARTED, PerHourEffect);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
			Messenger.AddListener(Signals.HOUR_STARTED, PerHourEffect);
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (owner.hasMarker)
		{
			owner.marker.visionColliderComponent.VoteToUnFilterVision();
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			int count = responsibleCharactersStack.Count;
			for (int i = 0; i < count; i++)
			{
				RemoveOldestCharacterFromStackList();
			}
			owner = null;
			if ((bool)character.marker)
			{
				character.marker.visionColliderComponent.VoteToFilterVision();
			}
			Messenger.RemoveListener(Signals.HOUR_STARTED, PerHourEffect);
		}
	}

	public override void AddCharacterResponsibleForTrait(Character character)
	{
		base.AddCharacterResponsibleForTrait(character);
		if (character != null)
		{
			AddCharacterToStackList(character);
		}
	}

	public override void OnUnstackStatus(ITraitable addedTo, bool bySchedule)
	{
		base.OnUnstackStatus(addedTo, bySchedule);
		RemoveOldestCharacterFromStackList();
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character character)
		{
			if (characterThatWillDoJob.moodComponent.moodState == MOOD_STATE.Critical)
			{
				int num = 0;
				if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(character, "Enemy"))
				{
					num = 10;
				}
				else if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(character, "Rival"))
				{
					num = 25;
				}
				if (UnityEngine.Random.Range(0, 100) < num)
				{
					characterThatWillDoJob.combatComponent.Fight(character, "Anger");
				}
			}
			else if (characterThatWillDoJob.moodComponent.moodState == MOOD_STATE.Bad)
			{
				int num2 = 0;
				if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(character, "Enemy"))
				{
					if (!character.traitContainer.HasTrait("Unconscious"))
					{
						num2 = 10;
					}
				}
				else if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(character, "Rival") && !character.traitContainer.HasTrait("Unconscious"))
				{
					num2 = 25;
				}
				if (UnityEngine.Random.Range(0, 100) < num2)
				{
					characterThatWillDoJob.combatComponent.Fight(character, "Anger", null, isLethal: false);
				}
			}
			else
			{
				int num3 = 2;
				if (UnityEngine.Random.Range(0, 100) < num3 && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(character) && !character.traitContainer.HasTrait("Unconscious"))
				{
					characterThatWillDoJob.combatComponent.Fight(character, "Anger", null, isLethal: false);
				}
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Angry angry)
		{
			_responsibleCharactersStack.AddRange(angry.responsibleCharactersStack);
		}
	}

	private void PerHourEffect()
	{
		if (!GameUtilities.RollChance(8) || !owner.limiterComponent.canPerform || !owner.limiterComponent.canMove || owner.isDead || owner == null || !owner.hasMarker || owner.marker.inVisionTileObjects.Count <= 0 || owner.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.ANGRY_DESTROY) || owner.jobQueue.HasJob(JOB_TYPE.ANGRY_DESTROY))
		{
			return;
		}
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < owner.marker.inVisionTileObjects.Count; i++)
		{
			TileObject tileObject = owner.marker.inVisionTileObjects[i];
			if (tileObject.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && tileObject.tileObjectType != TILE_OBJECT_TYPE.STRUCTURE_TILE_OBJECT && tileObject.gridTileLocation.GetFirstNeighborThatIsPassable() != null)
			{
				list.Add(tileObject);
			}
		}
		if (list.Count > 0)
		{
			TileObject randomElement = CollectionUtilities.GetRandomElement(list);
			owner.jobComponent.TriggerAngryDestroy(randomElement, "Destroy_Angry");
		}
		RuinarchListPool<TileObject>.Release(list);
	}

	private void AddCharacterToStackList(Character character)
	{
		responsibleCharactersStack.Add(character);
		owner?.relationshipContainer.AdjustOpinion(owner, character, "Anger", -30, "", createJobsOnReduce: false);
	}

	private void RemoveOldestCharacterFromStackList()
	{
		if (responsibleCharactersStack.Count > 0)
		{
			Character target = responsibleCharactersStack[0];
			responsibleCharactersStack.RemoveAt(0);
			owner.relationshipContainer.AdjustOpinion(owner, target, "Anger", 30);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
		_responsibleCharactersStack.Contains(p_character);
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		if (_responsibleCharactersStack.Contains(p_character))
		{
			_responsibleCharactersStack.Remove(p_character);
		}
	}
}
