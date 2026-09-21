using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public abstract class DefenseTower : ManMadeStructure
{
	public int remainingShots { get; private set; }

	public List<GameDate> remainingShotCooldowns { get; private set; }

	public List<Character> hostilesInRange { get; private set; }

	protected abstract int maxShots { get; }

	protected abstract int shotCooldown { get; }

	public override Type serializedData => typeof(SaveDataDefenseTower);

	protected DefenseTower(STRUCTURE_TYPE structureType, Region location)
		: base(structureType, location)
	{
		remainingShots = maxShots;
		remainingShotCooldowns = new List<GameDate>();
		hostilesInRange = new List<Character>();
	}

	protected DefenseTower(Region location, SaveDataDefenseTower data)
		: base(location, data)
	{
		remainingShots = data.remainingShots;
		remainingShotCooldowns = data.remainingShotCooldowns;
		hostilesInRange = new List<Character>();
	}

	public override void LoadStructureSecondWaveInMainThread(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadStructureSecondWaveInMainThread(saveDataLocationStructure);
		for (int i = 0; i < remainingShotCooldowns.Count; i++)
		{
			GameDate gameDate = remainingShotCooldowns[i];
			SchedulingManager.Instance.AddEntry(gameDate, ReplenishShot, this);
		}
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		base.SubscribeListeners(shouldLock);
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded, shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		if (structureObj is DefenseTowerStructureObject defenseTowerStructureObject)
		{
			defenseTowerStructureObject.SetConnectedTower(this);
		}
	}

	public override void ProcessOnSetAsActiveInStructureInfo()
	{
		base.ProcessOnSetAsActiveInStructureInfo();
		if (base.structureObj is DefenseTowerStructureObject defenseTowerStructureObject)
		{
			defenseTowerStructureObject.SetRangeHighlightState(p_state: true);
		}
	}

	public override void ProcessOnSetAsInactiveInStructureInfo()
	{
		base.ProcessOnSetAsInactiveInStructureInfo();
		if (base.structureObj is DefenseTowerStructureObject defenseTowerStructureObject)
		{
			defenseTowerStructureObject.SetRangeHighlightState(p_state: false);
		}
	}

	private void OnTickEnded()
	{
		if (hostilesInRange.Count > 0 && remainingShots > 0)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			PopulateValidHostilesInRange(list);
			Character randomElement = CollectionUtilities.GetRandomElement(list);
			if (randomElement != null)
			{
				Shoot(randomElement);
			}
			RuinarchListPool<Character>.Release(list);
		}
	}

	private void PopulateValidHostilesInRange(List<Character> p_choices)
	{
		for (int i = 0; i < hostilesInRange.Count; i++)
		{
			Character character = hostilesInRange[i];
			if (!character.traitContainer.HasTrait("Unconscious", "Restrained", "Prisoner") && character.carryComponent.isBeingCarriedBy == null)
			{
				p_choices.Add(character);
			}
		}
	}

	protected void Shoot(Character p_target)
	{
		remainingShots--;
		remainingShots = Mathf.Max(remainingShots, 0);
		CreateProjectile(p_target);
		GameDate gameDate = GameManager.Instance.Today();
		gameDate.AddTicks(shotCooldown);
		SchedulingManager.Instance.AddEntry(gameDate, ReplenishShot, this);
		remainingShotCooldowns.Add(gameDate);
	}

	private void ReplenishShot()
	{
		remainingShots++;
		remainingShots = Mathf.Min(remainingShots, maxShots);
		remainingShotCooldowns.RemoveAt(0);
	}

	protected abstract void CreateProjectile(IDamageable target);

	private void OnCharacterDied(Character p_character)
	{
		RemoveHostileInRange(p_character);
	}

	private void OnCharacterSwitchedFromLimbo(Character p_inLimbo, Character p_activeCharacter)
	{
		RemoveHostileInRange(p_inLimbo);
	}

	public void TryAddHostileInRange(Character p_character)
	{
		if (!hostilesInRange.Contains(p_character) && base.settlementLocation.owner != null && base.settlementLocation.owner.IsHostileWith(p_character.faction) && p_character.combatComponent.combatMode != COMBAT_MODE.Passive && !p_character.isDead)
		{
			hostilesInRange.Add(p_character);
		}
	}

	public void RemoveHostileInRange(Character p_character)
	{
		hostilesInRange.Remove(p_character);
	}

	public override string GetTestingInfo()
	{
		return string.Concat(string.Concat(base.GetTestingInfo() + "\nHostiles in range: " + hostilesInRange.ComafyList(), "\nRemaining shot cooldowns: ", remainingShotCooldowns.ComafyList()), "\nRemaining shots: ", remainingShots.ToString());
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			remainingShotCooldowns.Clear();
			hostilesInRange.Clear();
			base.CleanUp();
		}
	}
}
