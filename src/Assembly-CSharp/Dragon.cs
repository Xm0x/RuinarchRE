using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class Dragon : SkinnableAnimal
{
	private readonly int _leaveWorldTimer;

	public override Type serializedData => typeof(SaveDataDragon);

	public bool isAwakened { get; private set; }

	public bool isAttackingPlayer { get; private set; }

	public bool willLeaveWorld { get; private set; }

	public LocationStructure targetStructure { get; private set; }

	public int leaveWorldCounter { get; private set; }

	public List<Character> charactersThatAreWary { get; private set; }

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.DRAGON_HIDE;

	public override bool defaultDigMode => true;

	public Dragon()
		: base(SUMMON_TYPE.Dragon, "Dragon", RACE.DRAGON, Utilities.GetRandomGender())
	{
		isAwakened = true;
		base.traitContainer.AddTrait(this, "Fire Resistant");
		base.traitContainer.AddTrait(this, "Sturdy");
		_leaveWorldTimer = GameManager.Instance.GetTicksBasedOnHour(8);
		charactersThatAreWary = new List<Character>();
		for (int i = 20; i < 32; i++)
		{
			base.movementComponent.SetPenaltyForTag(i, 0);
		}
	}

	public Dragon(string className)
		: base(SUMMON_TYPE.Dragon, className, RACE.DRAGON, Utilities.GetRandomGender())
	{
		isAwakened = true;
		base.traitContainer.AddTrait(this, "Fire Resistant");
		base.traitContainer.AddTrait(this, "Sturdy");
		_leaveWorldTimer = GameManager.Instance.GetTicksBasedOnHour(8);
		charactersThatAreWary = new List<Character>();
	}

	public Dragon(SaveDataDragon data)
		: base(data)
	{
		charactersThatAreWary = new List<Character>();
		_leaveWorldTimer = GameManager.Instance.GetTicksBasedOnHour(8);
		isAwakened = data.isAwakened;
		isAttackingPlayer = data.isAttackingPlayer;
		willLeaveWorld = data.willLeaveWorld;
		leaveWorldCounter = data.leaveWorldCounter;
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetToFlying();
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
			Messenger.AddListener(Signals.TICK_STARTED, TryLeaveWorld);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
			Messenger.RemoveListener(Signals.TICK_STARTED, TryLeaveWorld);
		}
	}

	public override void DisconnectFromStructure(LocationStructure p_structure)
	{
		base.DisconnectFromStructure(p_structure);
		if (targetStructure == p_structure)
		{
			ResetTargetStructure();
		}
	}

	private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass newClass)
	{
		if (character == this && newClass.IsZombie())
		{
			Messenger.RemoveListener(Signals.TICK_STARTED, TryLeaveWorld);
		}
	}

	private void TryLeaveWorld()
	{
		if (isAwakened && !willLeaveWorld)
		{
			CheckLeaveWorld();
		}
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		if (data is SaveDataDragon saveDataDragon)
		{
			if (!string.IsNullOrEmpty(saveDataDragon.targetStructure))
			{
				targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataDragon.targetStructure);
			}
			if (saveDataDragon.charactersThatAreWary != null)
			{
				for (int i = 0; i < saveDataDragon.charactersThatAreWary.Count; i++)
				{
					Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(saveDataDragon.charactersThatAreWary[i]);
					if (characterByPersistentID != null)
					{
						charactersThatAreWary.Add(characterByPersistentID);
					}
				}
			}
		}
		base.LoadReferences(data);
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		charactersThatAreWary.Remove(p_character);
	}

	public override void CleanUp()
	{
		base.CleanUp();
		targetStructure = null;
		charactersThatAreWary?.Clear();
	}

	public void OnBecomeDefender()
	{
		SetWillLeaveWorld(state: false);
		Messenger.RemoveListener(Signals.TICK_STARTED, TryLeaveWorld);
	}

	public void Awaken()
	{
		if (!isAwakened)
		{
			isAwakened = true;
			base.traitContainer.RemoveTrait(this, "Immune");
			base.traitContainer.RemoveTrait(this, "Hibernating");
			AkSoundEngine.PostEvent("Play_Awaken_Dragon", InnerMapCameraMove.Instance.gameObject);
			Messenger.Broadcast(MonsterSignals.AWAKEN_DRAGON, (Character)this);
		}
	}

	public void SetIsAttackingPlayer(bool state)
	{
		isAttackingPlayer = state;
	}

	private void CheckLeaveWorld()
	{
		leaveWorldCounter++;
		if (leaveWorldCounter >= _leaveWorldTimer)
		{
			LeaveWorld();
		}
	}

	private void SetWillLeaveWorld(bool state)
	{
		if (willLeaveWorld != state)
		{
			willLeaveWorld = state;
			if (willLeaveWorld)
			{
				base.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
				base.jobQueue.CancelAllJobs();
				base.combatComponent.ClearHostilesInRange();
				base.combatComponent.ClearAvoidInRange();
			}
		}
	}

	private void LeaveWorld()
	{
		if (!base.isDead)
		{
			SetWillLeaveWorld(state: true);
		}
	}

	public void SetVillageTargetStructure()
	{
		targetStructure = base.gridTileLocation.GetNearestVillageStructureFromThisWithResidents(this);
	}

	public void SetPlayerTargetStructure()
	{
		targetStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
	}

	public void ResetTargetStructure()
	{
		targetStructure = null;
	}

	public void AddCharacterThatWary(Character character)
	{
		charactersThatAreWary.Add(character);
	}

	public override void OnMonsterCreatedForInitialWorldGeneration()
	{
		SpawnAsHibernating();
	}

	private void SpawnAsHibernating()
	{
		isAwakened = false;
		base.traitContainer.AddTrait(this, "Hibernating");
		base.traitContainer.AddTrait(this, "Immune");
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		_ = targetStructure;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		charactersThatAreWary.Contains(p_character);
	}
}
