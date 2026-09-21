using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class PrisonCell : StructureRoom
{
	private AutoDestroyParticle _particleEffect;

	public Character currentTortureTarget { get; private set; }

	public Character currentBrainwashTarget { get; private set; }

	public PrisonCell(List<LocationGridTile> tilesInRoom)
		: base("Prison Cell", tilesInRoom)
	{
		Vector3 centeredWorldLocation = GetCenterTile().centeredWorldLocation;
		centeredWorldLocation.x += 0.5f;
		base.worldPosition = centeredWorldLocation;
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.TORTURE, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.BRAINWASH, broadcastSignal);
	}

	public override void LoadReferences(SaveDataStructureRoom saveDataStructureRoom)
	{
		SaveDataPrisonCell saveDataPrisonCell = saveDataStructureRoom as SaveDataPrisonCell;
		if (!string.IsNullOrEmpty(saveDataPrisonCell.tortureID))
		{
			currentTortureTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataPrisonCell.tortureID);
		}
		else if (!string.IsNullOrEmpty(saveDataPrisonCell.brainwashTargetID))
		{
			currentBrainwashTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataPrisonCell.brainwashTargetID);
		}
		if (currentTortureTarget != null)
		{
			Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
		}
		else if (currentBrainwashTarget != null)
		{
			Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
		}
	}

	public override void LoadStructureRoomSecondWaveInMainThread(SaveDataStructureRoom saveDataStructureRoom)
	{
		base.LoadStructureRoomSecondWaveInMainThread(saveDataStructureRoom);
		if (currentTortureTarget != null)
		{
			LocationGridTile centerTile = GetCenterTile();
			_particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
		}
		else if (currentBrainwashTarget != null)
		{
			LocationGridTile centerTile2 = GetCenterTile();
			_particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile2.worldLocation, centerTile2.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
		}
	}

	public override bool CanUnseizeCharacterInRoom(Character character)
	{
		if (HasAnyAliveCharacterInRoom())
		{
			return false;
		}
		return IsValidOccupant(character);
	}

	public void OnHarpyDroppedCharacterHere(Character character)
	{
		if (IsValidOccupant(character))
		{
			character.traitContainer.RestrainAndImprison(character, null, PlayerManager.Instance.player.playerFaction);
		}
	}

	public bool IsValidOccupant(Character character)
	{
		if (character.isNormalCharacter && character.race != RACE.RATMAN && !character.isDead)
		{
			if (character.faction != null)
			{
				return !character.faction.isPlayerFaction;
			}
			return true;
		}
		return false;
	}

	public bool HasValidOccupant()
	{
		for (int i = 0; i < base.tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = base.tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if (IsValidOccupant(character))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasOccupants()
	{
		for (int i = 0; i < base.parentStructure.charactersHere.Count; i++)
		{
			Character character = base.parentStructure.charactersHere[i];
			if (character.gridTileLocation != null && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room == this)
			{
				return true;
			}
		}
		return false;
	}

	public void PopulateOccupants(List<Character> p_characters)
	{
		for (int i = 0; i < base.parentStructure.charactersHere.Count; i++)
		{
			Character character = base.parentStructure.charactersHere[i];
			if (character.gridTileLocation != null && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room == this)
			{
				p_characters.Add(character);
			}
		}
	}

	public void BeginTorture()
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < base.tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = base.tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if (IsValidTortureTarget(character))
				{
					list.Add(character);
				}
			}
		}
		if (list.Count > 0)
		{
			Character randomElement = CollectionUtilities.GetRandomElement(list);
			StartTorture(randomElement);
		}
		RuinarchListPool<Character>.Release(list);
	}

	public void BeginTorture(Character p_character)
	{
		StartTorture(p_character);
	}

	public bool HasValidTortureTarget()
	{
		for (int i = 0; i < base.tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = base.tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character p_character = locationGridTile.charactersHere[j];
				if (IsValidTortureTarget(p_character))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsValidTortureTarget(Character p_character)
	{
		if (p_character.isNormalCharacter && !p_character.isDead && !p_character.traitContainer.HasTrait("Being Drained"))
		{
			return !p_character.interruptComponent.isInterrupted;
		}
		return false;
	}

	private void StartTorture(Character target)
	{
		currentTortureTarget = target;
		currentTortureTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Tortured, currentTortureTarget);
		Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)base.parentStructure);
		LocationGridTile centerTile = GetCenterTile();
		_particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
	}

	private void StopTorture()
	{
		currentTortureTarget = null;
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)base.parentStructure);
	}

	private void CheckIfTortureInterruptFinished(INTERRUPT interrupt, Character character)
	{
		if (character == currentTortureTarget && interrupt == INTERRUPT.Being_Tortured)
		{
			_particleEffect.StopEmission();
			_particleEffect = null;
			_ = base.parentStructure;
			Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
			character.traitContainer.RestrainAndImprison(character, null, PlayerManager.Instance.player.playerFaction);
			StopTorture();
		}
	}

	public bool WasBrainwashSuccessful(Character actor)
	{
		if (actor.classComponent.IsStalkerCannotBeTurned())
		{
			return false;
		}
		WeightedDictionary<bool> weightedDictionary = new WeightedDictionary<bool>();
		GetBrainwashSuccessAndFailWeights(actor, out var successWeight, out var failWeight);
		weightedDictionary.AddElement(newElement: true, successWeight);
		weightedDictionary.AddElement(newElement: false, failWeight);
		return weightedDictionary.PickRandomElementGivenWeights();
	}

	private static void GetBrainwashSuccessAndFailWeights(Character actor, out int successWeight, out int failWeight)
	{
		if (actor.classComponent.IsStalkerCannotBeTurned())
		{
			failWeight = 100;
			successWeight = 0;
			return;
		}
		failWeight = 100;
		successWeight = 50;
		if (actor.moodComponent.moodState == MOOD_STATE.Normal)
		{
			if (actor.traitContainer.HasTrait("Evil"))
			{
				successWeight += 100;
			}
			if (actor.traitContainer.HasTrait("Treacherous"))
			{
				successWeight += 100;
			}
		}
		else if (actor.moodComponent.moodState == MOOD_STATE.Bad || actor.moodComponent.moodState == MOOD_STATE.Critical)
		{
			if (actor.moodComponent.moodState == MOOD_STATE.Bad)
			{
				successWeight += 100;
			}
			else if (actor.moodComponent.moodState == MOOD_STATE.Critical)
			{
				successWeight += 200;
			}
			if (actor.traitContainer.HasTrait("Evil"))
			{
				successWeight += 150;
			}
			if (actor.traitContainer.HasTrait("Treacherous"))
			{
				successWeight += 300;
			}
		}
		if (actor.traitContainer.HasTrait("Betrayed"))
		{
			successWeight += 100;
		}
		if (actor.isFactionLeader)
		{
			failWeight += 600;
		}
		if (actor.isSettlementRuler)
		{
			failWeight += 600;
		}
		if (actor.characterClass.className == "Priest" || actor.characterClass.className == "Great Witch")
		{
			failWeight += 600;
		}
		if (actor.characterClass.className == "Hero" || actor.traitContainer.IsBlessed())
		{
			successWeight = 0;
			failWeight = 100;
		}
		if (actor.traitContainer.HasTrait("Devout"))
		{
			if (actor.religionComponent.religion == RELIGION.Demon_Worship)
			{
				successWeight = 100;
				failWeight = 0;
			}
			else
			{
				successWeight = 0;
				failWeight = 100;
			}
		}
	}

	public static float GetBrainwashSuccessRate(Character character)
	{
		GetBrainwashSuccessAndFailWeights(character, out var successWeight, out var failWeight);
		return (float)successWeight / (float)(successWeight + failWeight) * 100f;
	}

	public bool HasValidBrainwashTarget()
	{
		for (int i = 0; i < base.tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = base.tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character p_character = locationGridTile.charactersHere[j];
				if (IsValidBrainwashTarget(p_character))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PopulateBrainwashTargets(List<Character> p_characters)
	{
		for (int i = 0; i < base.tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = base.tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if (IsValidBrainwashTarget(character))
				{
					p_characters.Add(character);
				}
			}
		}
	}

	public bool IsValidBrainwashTarget(Character p_character)
	{
		if (p_character.isNormalCharacter && !p_character.isDead && !p_character.traitContainer.HasTrait("Demon Cultist") && !p_character.traitContainer.IsBlessed() && !p_character.traitContainer.HasTrait("Being Drained"))
		{
			return !p_character.interruptComponent.isInterrupted;
		}
		return false;
	}

	public void StartBrainwash()
	{
		GetTileObjectInRoom<DoorTileObject>()?.Close();
		Character character = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		PopulateBrainwashTargets(list);
		character = CollectionUtilities.GetRandomElement(list);
		RuinarchListPool<Character>.Release(list);
		currentBrainwashTarget = character;
		currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
		currentBrainwashTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Brainwashed, currentBrainwashTarget);
		Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)base.parentStructure);
		LocationGridTile centerTile = GetCenterTile();
		_particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
	}

	public void StartBrainwash(Character p_target)
	{
		GetTileObjectInRoom<DoorTileObject>()?.Close();
		currentBrainwashTarget = p_target;
		currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
		currentBrainwashTarget.interruptComponent.TriggerInterrupt(INTERRUPT.Being_Brainwashed, currentBrainwashTarget);
		Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)base.parentStructure);
		LocationGridTile centerTile = GetCenterTile();
		_particleEffect = GameManager.Instance.CreateParticleEffectAt(centerTile.worldLocation, centerTile.parentMap, PARTICLE_EFFECT.Torture_Cloud).GetComponent<AutoDestroyParticle>();
	}

	private void BrainwashDone()
	{
		currentBrainwashTarget = null;
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)base.parentStructure);
	}

	private void CheckIfBrainwashFinished(INTERRUPT interrupt, Character chosenTarget)
	{
		if (interrupt == INTERRUPT.Being_Brainwashed && chosenTarget == currentBrainwashTarget)
		{
			Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
			_particleEffect.StopEmission();
			_particleEffect = null;
			if (chosenTarget.traitContainer.HasTrait("Demon Cultist"))
			{
				chosenTarget.traitContainer.RemoveTrait(chosenTarget, "Unconscious");
			}
			else
			{
				chosenTarget.traitContainer.RestrainAndImprison(chosenTarget, null, PlayerManager.Instance.player.playerFaction);
			}
			BrainwashDone();
		}
	}

	public override void OnParentStructureDestroyed()
	{
		base.OnParentStructureDestroyed();
		Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfTortureInterruptFinished);
		if (currentTortureTarget != null && currentTortureTarget.interruptComponent.isInterrupted && currentTortureTarget.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured)
		{
			currentTortureTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
		}
		if (_particleEffect != null)
		{
			_particleEffect.StopEmission();
			_particleEffect = null;
		}
		Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfBrainwashFinished);
		if (currentBrainwashTarget != null && currentBrainwashTarget.interruptComponent.isInterrupted && currentBrainwashTarget.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed)
		{
			currentBrainwashTarget.interruptComponent.ForceEndNonSimultaneousInterrupt();
		}
	}

	public override bool CanBeSelected()
	{
		return false;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = currentTortureTarget;
		_ = currentBrainwashTarget;
	}
}
