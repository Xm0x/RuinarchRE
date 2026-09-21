using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class Ghost : Summon
{
	public override Type serializedData => typeof(SaveDataGhost);

	public Character betrayedBy { get; private set; }

	public override Faction defaultFaction => FactionManager.Instance.undeadFaction;

	public Ghost()
		: base(SUMMON_TYPE.Ghost, "Ghost", RACE.GHOST, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
	}

	public Ghost(string className)
		: base(SUMMON_TYPE.Ghost, className, RACE.GHOST, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
	}

	public Ghost(SaveDataGhost data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetToFlying();
		RemoveAdvertisedAction(INTERACTION_TYPE.BURY_CHARACTER);
		base.isWildMonster = false;
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
			Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
			Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
		}
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		if (data is SaveDataGhost saveDataGhost && !string.IsNullOrEmpty(saveDataGhost.betrayedBy))
		{
			betrayedBy = CharacterManager.Instance.GetCharacterByPersistentID(saveDataGhost.betrayedBy);
		}
		base.LoadReferences(data);
	}

	public override void LoadReferencesMainThread(SaveDataCharacter data)
	{
		base.LoadReferencesMainThread(data);
		base.visuals.SetHasBlood(state: false);
	}

	protected override void OnTickEnded()
	{
		base.OnTickEnded();
		PerTickOutsideCombatHPRecovery();
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		if (betrayedBy == p_character)
		{
			SetBetrayedBy(null);
		}
	}

	public void SetBetrayedBy(Character character)
	{
		betrayedBy = character;
	}

	private void OnCharacterStartedState(Character character, CharacterState state)
	{
		if (character == this && state.characterState == CHARACTER_STATE.COMBAT)
		{
			Messenger.AddListener(Signals.TICK_ENDED, FearCheck);
		}
	}

	private void OnCharacterEndedState(Character character, CharacterState state)
	{
		if (character == this && state.characterState == CHARACTER_STATE.COMBAT)
		{
			Messenger.RemoveListener(Signals.TICK_ENDED, FearCheck);
		}
	}

	private void FearCheck()
	{
		if (base.hasBeenCleanedUp)
		{
			Messenger.RemoveListener(Signals.TICK_ENDED, FearCheck);
		}
		else
		{
			if (!Utilities.IsEven(GameManager.Instance.Today().tick) || UnityEngine.Random.Range(0, 100) >= 15)
			{
				return;
			}
			List<Character> list = new List<Character>();
			for (int i = 0; i < base.combatComponent.hostilesInRange.Count; i++)
			{
				if (base.combatComponent.hostilesInRange[i] is Character character && !character.marker.hasFleePath && (!character.interruptComponent.isInterrupted || character.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Cowering))
				{
					list.Add(character);
				}
			}
			for (int j = 0; j < base.combatComponent.avoidInRange.Count; j++)
			{
				if (base.combatComponent.avoidInRange[j] is Character character2 && !character2.marker.hasFleePath && (!character2.interruptComponent.isInterrupted || character2.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Cowering))
				{
					list.Add(character2);
				}
			}
			if (list.Count > 0)
			{
				Character randomElement = CollectionUtilities.GetRandomElement(list);
				randomElement.interruptComponent.TriggerInterrupt(INTERRUPT.Feared, this);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Summon", "CharacterAlerts_Table", "cast_fear", LOG_TAG.Combat);
				log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(randomElement, randomElement.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
		}
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Attack_Villager_Tooltip.ToStringEnum();
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = betrayedBy;
	}

	public override void CleanUp()
	{
		base.CleanUp();
		Messenger.RemoveListener(Signals.TICK_ENDED, FearCheck);
		betrayedBy = null;
	}
}
