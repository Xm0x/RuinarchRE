using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CombatComponent : CharacterComponent
{
	public struct DamageDoneType
	{
		public enum DamageType
		{
			Normal,
			Crit
		}

		public int amount;

		public DamageType damageType;
	}

	public List<ELEMENTAL_TYPE> elementalStatusWaitingList = new List<ELEMENTAL_TYPE>(10);

	public DamageDoneType damageDone;

	public int attack { get; private set; }

	public int strengthModification { get; private set; }

	public float strengthPercentModification { get; private set; }

	public int intelligenceModification { get; private set; }

	public float intelligencePercentModification { get; private set; }

	public int maxHP { get; private set; }

	public int maxHPModification { get; private set; }

	public float maxHPPercentModification { get; private set; }

	public int attackSpeed { get; private set; }

	public float attackSpeedPercentModification { get; private set; }

	public int numOfKilledCharacters { get; private set; }

	public COMBAT_MODE combatMode { get; private set; }

	public COMBAT_MODE previousCombatMode { get; private set; }

	public List<IPointOfInterest> hostilesInRange { get; private set; }

	public List<IPointOfInterest> avoidInRange { get; private set; }

	public List<Character> bannedFromHostileList { get; private set; }

	public List<Character> unkillableCharacters { get; private set; }

	public Dictionary<IPointOfInterest, CombatData> combatDataDictionary { get; private set; }

	public ElementalDamageData currentElement { get; private set; }

	public ELEMENTAL_TYPE previousElementType { get; private set; }

	public CharacterCombatBehaviourParent combatBehaviourParent { get; private set; }

	public CombatSpecialSkillWrapper specialSkillParent { get; private set; }

	public bool willProcessCombat { get; private set; }

	public int critRate { get; private set; }

	public int clearUnkillableListTicks { get; private set; }

	public bool shouldIncreaseClearUnkillableListTicks { get; private set; }

	public int hpRecoveryPerTickOutsideCombat { get; private set; }

	public bool isInCombat
	{
		get
		{
			if (base.owner.stateComponent.currentState != null)
			{
				return base.owner.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT;
			}
			return false;
		}
	}

	public bool isInActualCombat => IsInActualCombat();

	public int unModifiedMaxHP => Mathf.RoundToInt((float)base.owner.characterClass.baseHP * ((base.owner.raceSetting.hpMultiplier == 0f) ? 1f : base.owner.raceSetting.hpMultiplier));

	public int unModifiedAttack => Mathf.RoundToInt((float)base.owner.characterClass.baseAttackPower * ((base.owner.raceSetting.attackMultiplier == 0f) ? 1f : base.owner.raceSetting.attackMultiplier));

	public RANGE_TYPE rangeType
	{
		get
		{
			if (!(base.owner is Dragon))
			{
				return base.owner.characterClass.rangeType;
			}
			return RANGE_TYPE.RANGED;
		}
	}

	public float attackRange => GetAttackRange();

	public CombatComponent()
	{
		hostilesInRange = new List<IPointOfInterest>(20);
		avoidInRange = new List<IPointOfInterest>(20);
		bannedFromHostileList = new List<Character>(20);
		unkillableCharacters = new List<Character>(20);
		combatDataDictionary = new Dictionary<IPointOfInterest, CombatData>(20);
		specialSkillParent = new CombatSpecialSkillWrapper();
		combatBehaviourParent = new CharacterCombatBehaviourParent();
		SetCombatMode(COMBAT_MODE.Aggressive);
	}

	public CombatComponent(SaveDataCombatComponent data)
	{
		hostilesInRange = new List<IPointOfInterest>(20);
		avoidInRange = new List<IPointOfInterest>(20);
		bannedFromHostileList = new List<Character>(20);
		unkillableCharacters = new List<Character>(20);
		combatDataDictionary = new Dictionary<IPointOfInterest, CombatData>(20);
		attack = data.attack;
		strengthModification = data.strengthModification;
		critRate = data.critRate;
		strengthPercentModification = data.strengthPercentModification;
		intelligenceModification = data.intelligenceModification;
		intelligencePercentModification = data.intelligencePercentModification;
		maxHP = data.maxHP;
		maxHPModification = data.maxHPModification;
		maxHPPercentModification = data.maxHPPercentModification;
		attackSpeed = data.attackSpeed;
		attackSpeedPercentModification = data.attackSpeedPercentModification;
		combatMode = data.combatMode;
		previousCombatMode = data.previousCombatMode;
		currentElement = ScriptableObjectsManager.Instance.GetElementalDamageData(data.elementalDamageType);
		elementalStatusWaitingList = new List<ELEMENTAL_TYPE>(data.elementalStatusWaitingList);
		willProcessCombat = data.willProcessCombat;
		numOfKilledCharacters = data.numOfKilledCharacters;
		clearUnkillableListTicks = data.clearUnkillableListTicks;
		shouldIncreaseClearUnkillableListTicks = data.shouldIncreaseClearUnkillableListTicks;
		hpRecoveryPerTickOutsideCombat = data.hpRecoveryPerTickOutsideCombat;
		specialSkillParent = data.specialSkillParent.Load();
		combatBehaviourParent = data.combatBehaviourParent.Load();
	}

	public void SubscribeToSignals()
	{
		Messenger.AddListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
		Messenger.AddListener<MovingTileObject>(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, OnMovingTileObjectExpired);
		Messenger.AddListener<Character>(CharacterSignals.ON_CHARACTER_TAMED, OnBeastTamed);
	}

	public void UnsubscribeToSignals()
	{
		Messenger.RemoveListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
		Messenger.RemoveListener<MovingTileObject>(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, OnMovingTileObjectExpired);
		Messenger.RemoveListener<Character>(CharacterSignals.ON_CHARACTER_TAMED, OnBeastTamed);
	}

	private void OnHasBecomePrisoner(Prisoner prisoner)
	{
		OnCharacterBecomePrisoner(prisoner);
	}

	private void OnMovingTileObjectExpired(MovingTileObject p_tileObject)
	{
		if (IsAvoidInRange(p_tileObject))
		{
			RemoveAvoidInRange(p_tileObject);
		}
	}

	public void OnTickEnded()
	{
		IncreaseClearUnkillableListTicks();
	}

	private void OnBeastTamed(Character p_character)
	{
		if (!base.owner.IsHostileWith(p_character) && (IsHostileInRange(p_character) || IsAvoidInRange(p_character)))
		{
			RemoveAvoidInRange(p_character);
			RemoveHostileInRange(p_character);
		}
	}

	public void LevelUpBaseOnPrimordialBonusUpgrade(CHARACTER_CATEGORY p_category, PRIMORDIAL_STATS_BONUS p_bonus)
	{
		if (!(PlayerManager.Instance == null) && PlayerManager.Instance.player != null && PlayerManager.Instance.player.primordialPoolDataHandler != null && (!PlayerManager.Instance.player.retaliationComponent.HasRetaliator(base.owner) || !base.owner.behaviourComponent.HasBehaviour(typeof(AttackDemonicStructureBehaviour))))
		{
			switch (p_bonus)
			{
			case PRIMORDIAL_STATS_BONUS.Str:
				AdjustStrengthPercentModifier(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetCurrentBonus(p_bonus));
				AdjustIntelligencePercentModifier(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetCurrentBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Piercing:
				base.owner.piercingAndResistancesComponent.AdjustBasePiercing(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetCurrentBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Mental_Res:
				base.owner.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetCurrentBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Physical_Res:
				base.owner.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetCurrentBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Elemental_Res:
				base.owner.piercingAndResistancesComponent.AdjustElementalResistances(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetCurrentBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Secondary_Res:
				base.owner.piercingAndResistancesComponent.AdjustSecondaryResistances(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetCurrentBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Int:
				break;
			}
		}
	}

	public void LevelUpBaseOnInitializeAndApplyAllLevelBonus(CHARACTER_CATEGORY p_category, PRIMORDIAL_STATS_BONUS p_bonus)
	{
		if (!(PlayerManager.Instance == null) && PlayerManager.Instance.player != null && PlayerManager.Instance.player.primordialPoolDataHandler != null && (!PlayerManager.Instance.player.retaliationComponent.HasRetaliator(base.owner) || !base.owner.behaviourComponent.HasBehaviour(typeof(AttackDemonicStructureBehaviour))))
		{
			switch (p_bonus)
			{
			case PRIMORDIAL_STATS_BONUS.Str:
				AdjustStrengthPercentModifier(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetAllBonus(p_bonus));
				AdjustIntelligencePercentModifier(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetAllBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Piercing:
				base.owner.piercingAndResistancesComponent.AdjustBasePiercing(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetAllBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Mental_Res:
				base.owner.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetAllBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Physical_Res:
				base.owner.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetAllBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Elemental_Res:
				base.owner.piercingAndResistancesComponent.AdjustElementalResistances(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetAllBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Secondary_Res:
				base.owner.piercingAndResistancesComponent.AdjustSecondaryResistances(PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_category].GetAllBonus(p_bonus));
				break;
			case PRIMORDIAL_STATS_BONUS.Int:
				break;
			}
		}
	}

	public void ApplyInitialBonusStrengthAndIntelligenceOnCreation()
	{
	}

	public int GetAttackWithCritRateBonus(IPointOfInterest p_hitPOI)
	{
		return GetAttackWithCritRateBonus(p_hitPOI, attack);
	}

	public int GetAttackWithCritRateBonus(IPointOfInterest p_hitPOI, int attack)
	{
		int num = 1;
		if (WillAttackBeACriticalStrike(p_hitPOI))
		{
			num = 2;
			damageDone.damageType = DamageDoneType.DamageType.Crit;
		}
		else
		{
			damageDone.damageType = DamageDoneType.DamageType.Normal;
		}
		int num2 = attack * num;
		damageDone.amount = num2;
		return num2;
	}

	public int GetAttackWithCritAndModifications(IPointOfInterest p_hitPOI, int p_rawAttackValue)
	{
		int attackPower = GetAttackWithCritRateBonus(p_hitPOI, p_rawAttackValue);
		if (p_hitPOI is Character character)
		{
			character.classComponent.ProcessHittingHunterByHisHunterSpecialization(base.owner, ref attackPower);
			if (base.owner.traitContainer.HasTrait("Religion Buff") && character.religionComponent.religion != base.owner.religionComponent.religion)
			{
				attackPower *= 2;
			}
		}
		else if (p_hitPOI is TileObject && base.owner.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Razer))
		{
			attackPower = Mathf.RoundToInt((float)attackPower * 1.5f);
		}
		if (base.owner.equipmentComponent.currentWeapon is WeaponItem weaponItem)
		{
			weaponItem.ApplyAttackPowerChanges(p_hitPOI, ref attackPower);
		}
		if (base.owner.traitContainer.HasTrait("Injured"))
		{
			attackPower = Mathf.RoundToInt((float)attackPower * 0.5f);
		}
		return attackPower;
	}

	private bool WillAttackBeACriticalStrike(IPointOfInterest p_hitPOI)
	{
		if (base.owner.classComponent.WillAttackBeACriticalStrikeBasedOnHunterSpecialization(p_hitPOI))
		{
			return true;
		}
		if (base.owner.classComponent.DoesStalkerDealDoubleDamage() && p_hitPOI is Character p_target && (base.owner.classComponent.IsTargetRecognizedAsVampireByStalker(p_target) || base.owner.classComponent.IsTargetRecognizedAsLycanByStalker(p_target) || base.owner.classComponent.IsTargetRecognizedAsCultistByStalker(p_target)))
		{
			return true;
		}
		int num = critRate;
		if (p_hitPOI.traitContainer.HasTrait("Hermit"))
		{
			num -= 30;
		}
		return GameUtilities.RollChance(num);
	}

	public void ResetDamageDoneType()
	{
		damageDone.damageType = DamageDoneType.DamageType.Normal;
	}

	private void ProcessCombatBehavior()
	{
		if (!base.owner.interruptComponent.isInterrupted)
		{
			if (base.owner.combatComponent.isInCombat)
			{
				Messenger.Broadcast(CharacterSignals.DETERMINE_COMBAT_REACTION, base.owner);
			}
			else if ((hostilesInRange.Count > 0 || avoidInRange.Count > 0) && !base.owner.jobQueue.HasJob(JOB_TYPE.COMBAT))
			{
				CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, base.owner);
				base.owner.jobQueue.AddJobInQueue(job);
			}
		}
	}

	public void CheckCombatPerTickEnded()
	{
		if (willProcessCombat)
		{
			SetWillProcessCombat(state: false);
			ProcessCombatBehavior();
		}
	}

	public void SetCombatMode(COMBAT_MODE mode)
	{
		previousCombatMode = combatMode;
		combatMode = mode;
	}

	public void SetElementalType(ELEMENTAL_TYPE elementalType)
	{
		if (!(currentElement == null) && currentElement.type == elementalType)
		{
			return;
		}
		if (currentElement == null)
		{
			previousElementType = ELEMENTAL_TYPE.Normal;
		}
		else
		{
			previousElementType = currentElement.type;
		}
		currentElement = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
		List<Trait> traitOverrideFunctions = base.owner.traitContainer.GetTraitOverrideFunctions("Change_Element");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].OnChangeElement(base.owner, currentElement.type, previousElementType);
			}
		}
	}

	public void UpdateElementalType()
	{
		if (!base.owner.equipmentComponent.HasEquips())
		{
			bool flag = false;
			if (elementalStatusWaitingList.Count > 0)
			{
				int index = Random.Range(0, elementalStatusWaitingList.Count);
				flag = true;
				SetElementalType(elementalStatusWaitingList[index]);
			}
			if (!flag)
			{
				SetElementalType(base.owner.characterClass.elementalType);
			}
		}
	}

	public void SetWillProcessCombat(bool state)
	{
		willProcessCombat = state;
	}

	private bool IsInActualCombat()
	{
		if ((bool)base.owner.marker && base.owner.stateComponent.currentState != null && base.owner.stateComponent.currentState is CombatState combatState)
		{
			if (!combatState.isAttacking)
			{
				return true;
			}
			if (combatState.currentClosestHostile != null)
			{
				if (base.owner.marker.IsPOIInVision(combatState.currentClosestHostile))
				{
					return true;
				}
				if (base.owner.marker.inVisionPOIsButDiffStructure.Contains(combatState.currentClosestHostile))
				{
					return true;
				}
				if (combatState.currentClosestHostile is Character character && character.combatComponent.isInCombat && !(character.stateComponent.currentState as CombatState).isAttacking)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsInActualCombatWith(IPointOfInterest target)
	{
		if ((bool)base.owner.marker && base.owner.stateComponent.currentState != null && base.owner.stateComponent.currentState is CombatState { isAttacking: not false, currentClosestHostile: not null } combatState && combatState.currentClosestHostile == target)
		{
			if (combatState.currentClosestHostile is Character poi)
			{
				if (base.owner.marker.IsPOIInVision(poi))
				{
					return true;
				}
			}
			else if (combatState.currentClosestHostile is TileObject poi2 && base.owner.marker.IsPOIInVision(poi2))
			{
				return true;
			}
			if (base.owner.marker.inVisionPOIsButDiffStructure.Contains(combatState.currentClosestHostile))
			{
				return true;
			}
		}
		return false;
	}

	public CombatReaction GetFightOrFlightReaction(IPointOfInterest target, string fightReason)
	{
		_ = string.Empty;
		if (!base.owner.limiterComponent.canPerform || !base.owner.limiterComponent.canMove)
		{
			return new CombatReaction(COMBAT_REACTION.None);
		}
		if (IsHostileInRange(target) || IsAvoidInRange(target))
		{
			return new CombatReaction(COMBAT_REACTION.None);
		}
		if (base.owner.behaviourComponent.HasBehaviour(typeof(DisablerBehaviour)))
		{
			return new CombatReaction(COMBAT_REACTION.Flight);
		}
		if (target is Character character)
		{
			if (base.owner.traitContainer.HasTrait("Enslaved") && base.owner.isNormalCharacter && character.isNormalCharacter)
			{
				return new CombatReaction(COMBAT_REACTION.Flight);
			}
			if ((base.owner.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Glass_Cannon) || base.owner.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Healer)) && base.owner.partyComponent.HasReachablePartymateToFleeTo() && !base.owner.partyComponent.HasPartymateInVision())
			{
				return new CombatReaction(COMBAT_REACTION.Flight, "Vulnerable");
			}
		}
		if (base.owner.traitContainer.HasTrait("Berserked") || base.owner is Summon || base.owner.characterClass.IsZombie() || base.owner.race == RACE.DEMON)
		{
			return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
		}
		if (base.owner.race == RACE.RATMAN)
		{
			Faction faction = base.owner.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Ratmen)
			{
				if (base.owner.gridTileLocation != null && base.owner.gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.owner != null && settlement.owner != base.owner.faction)
				{
					return new CombatReaction(COMBAT_REACTION.Flight);
				}
				return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
			}
		}
		if (base.owner.traitContainer.HasTrait("Drunk"))
		{
			if (GameUtilities.RollChance(50))
			{
				return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
			}
			return new CombatReaction(COMBAT_REACTION.Flight);
		}
		if (target is TileObject tileObject)
		{
			if (base.owner.traitContainer.HasTrait("Coward"))
			{
				if (!base.owner.traitContainer.GetTraitOrStatus<Coward>("Coward").TryActivatePassOut(base.owner))
				{
					return new CombatReaction(COMBAT_REACTION.Flight, "Coward");
				}
				return default(CombatReaction);
			}
			if (tileObject.traitContainer.HasTrait("Dangerous"))
			{
				if (!string.IsNullOrEmpty(tileObject.neutralizer) && base.owner.traitContainer.HasTrait(tileObject.neutralizer))
				{
					return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
				}
				return new CombatReaction(COMBAT_REACTION.Flight);
			}
			return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
		}
		if (target is Character character2)
		{
			if (base.owner.faction != null && character2.faction != null && !base.owner.faction.IsHostileWith(character2.faction) && !character2.combatComponent.IsHostileInRange(base.owner))
			{
				string opinionLabel = base.owner.relationshipContainer.GetOpinionLabel(character2);
				if (opinionLabel == "Close Friend")
				{
					return new CombatReaction(COMBAT_REACTION.Flight);
				}
				if (opinionLabel == "Friend" && GameUtilities.RollChance(75))
				{
					return new CombatReaction(COMBAT_REACTION.Flight);
				}
			}
			if (!base.owner.characterClass.IsCombatant() && !(base.owner.characterClass.className == "Noble"))
			{
				if (base.owner.traitContainer.HasTrait("Coward"))
				{
					if (!base.owner.traitContainer.GetTraitOrStatus<Coward>("Coward").TryActivatePassOut(base.owner))
					{
						return new CombatReaction(COMBAT_REACTION.Flight, "Coward");
					}
					return default(CombatReaction);
				}
				if (!character2.characterClass.IsCombatant() && !(character2.characterClass.className == "Noble"))
				{
					return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
				}
				if (HasCharacterInVisionWithSameHostile(character2) && base.owner.IsInHomeSettlement())
				{
					return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
				}
				if (character2.characterClass.className == "Noble")
				{
					return new CombatReaction(COMBAT_REACTION.Flight);
				}
				if (GameUtilities.RollChance(95))
				{
					return new CombatReaction(COMBAT_REACTION.Flight);
				}
				return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
			}
			if (CombatManager.Instance.IsImmuneToElement(character2, currentElement.type))
			{
				if (HasCharacterInVisionWithSameHostile(character2))
				{
					return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
				}
				return new CombatReaction(COMBAT_REACTION.Flight);
			}
			if (base.owner.traitContainer.HasTrait("Coward", "Vampire") && base.owner.currentHP <= Mathf.CeilToInt((float)base.owner.maxHP * 0.2f))
			{
				Coward traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Coward>("Coward");
				if (traitOrStatus != null)
				{
					if (!traitOrStatus.TryActivatePassOut(base.owner))
					{
						return new CombatReaction(COMBAT_REACTION.Flight, "Coward");
					}
					return default(CombatReaction);
				}
				Vampire traitOrStatus2 = base.owner.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus2 != null && traitOrStatus2.CanTransformIntoBat())
				{
					return new CombatReaction(COMBAT_REACTION.Flight, "can escape as a vampire bat");
				}
				return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
			}
			return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
		}
		return new CombatReaction(COMBAT_REACTION.None);
	}

	public void FightOrFlight(IPointOfInterest target, CombatReaction combatReaction, ActualGoapNode connectedAction = null, bool isLethal = true, bool willAttackBecauseOfCrime = false)
	{
		if (combatReaction.reaction == COMBAT_REACTION.Fight)
		{
			Fight(target, combatReaction.reason, connectedAction, isLethal, willAttackBecauseOfCrime);
		}
		else if (combatReaction.reaction == COMBAT_REACTION.Flight)
		{
			if (base.owner.movementComponent.isStationary)
			{
				Fight(target, combatReaction.reason, connectedAction, isLethal, willAttackBecauseOfCrime);
			}
			else
			{
				Flight(target, combatReaction.reason);
			}
		}
	}

	public void FightOrFlight(IPointOfInterest target, string fightReason, ActualGoapNode connectedAction = null, bool isLethal = true, bool willAttackBecauseOfCrime = false)
	{
		CombatReaction fightOrFlightReaction = GetFightOrFlightReaction(target, fightReason);
		if (fightOrFlightReaction.reaction != COMBAT_REACTION.None)
		{
			FightOrFlight(target, fightOrFlightReaction, connectedAction, isLethal, willAttackBecauseOfCrime);
		}
	}

	public bool Fight(IPointOfInterest target, string reason, ActualGoapNode connectedAction = null, bool isLethal = true, bool willAttackBecauseOfCrime = false, bool bypassBannedHostiles = false)
	{
		_ = string.Empty;
		bool result = false;
		bool flag = false;
		if (!bypassBannedHostiles)
		{
			flag = (reason == "Hostility" && target is Character item && bannedFromHostileList.Contains(item)) || !base.owner.limiterComponent.canPerform;
		}
		if (!flag && !IsHostileInRange(target))
		{
			hostilesInRange.Add(target);
			avoidInRange.Remove(target);
			SetWillProcessCombat(state: true);
			if (combatDataDictionary.ContainsKey(target))
			{
				combatDataDictionary[target].SetFightData(reason, connectedAction, isLethal, willAttackBecauseOfCrime);
			}
			else
			{
				CombatData combatData = ObjectPoolManager.Instance.CreateNewCombatData();
				combatData.SetFightData(reason, connectedAction, isLethal, willAttackBecauseOfCrime);
				combatDataDictionary.Add(target, combatData);
			}
			if (target is TileObject tileObject)
			{
				tileObject.AdjustRepairCounter(1);
			}
			target.CancelRemoveStatusFeedAndRepairJobsTargetingThis();
			result = true;
		}
		return result;
	}

	public bool Flight(IPointOfInterest target, string reason = "")
	{
		if (!base.owner.hasMarker || base.owner.isDead)
		{
			return false;
		}
		if (base.owner.movementComponent.isStationary)
		{
			return false;
		}
		bool result = false;
		if (hostilesInRange.Remove(target))
		{
			if (target is TileObject tileObject)
			{
				tileObject.AdjustRepairCounter(-1);
			}
			else if (target is Character targetCharacter)
			{
				AddPOIToBannedFromHostile(targetCharacter);
			}
			if (target is TileObject { gridTileLocation: not null } tileObject2 && tileObject2.gridTileLocation.structure is DemonicStructure demonicStructure)
			{
				demonicStructure.RemoveAttacker(base.owner);
			}
		}
		_ = string.Empty;
		if (base.owner.limiterComponent.canMove)
		{
			if (!IsAvoidInRange(target) && base.owner.hasMarker && base.owner.marker.IsPOIInVision(target))
			{
				avoidInRange.Add(target);
				SetWillProcessCombat(state: true);
				if (combatDataDictionary.ContainsKey(target))
				{
					combatDataDictionary[target].SetFlightData(reason);
				}
				else
				{
					CombatData combatData = ObjectPoolManager.Instance.CreateNewCombatData();
					combatData.SetFlightData(reason);
					combatDataDictionary.Add(target, combatData);
				}
				result = true;
				if (target is Character)
				{
					Character character = target as Character;
					if (character.combatComponent.combatMode == COMBAT_MODE.Defend && (character.currentJob == null || character.currentJob.jobType != JOB_TYPE.MONSTER_ABDUCT))
					{
						character.combatComponent.RemoveHostileInRange(base.owner);
					}
				}
			}
		}
		else if (target is Character)
		{
			Character character2 = target as Character;
			if (character2.combatComponent.combatMode == COMBAT_MODE.Defend)
			{
				character2.combatComponent.RemoveHostileInRange(base.owner);
			}
		}
		return result;
	}

	public void FlightAll(string reason = "")
	{
		if (base.owner.movementComponent.isStationary || base.owner.race == RACE.DEMON || base.owner.race == RACE.DRAGON || hostilesInRange.Count <= 0)
		{
			return;
		}
		if (base.owner.limiterComponent.canMove)
		{
			for (int i = 0; i < hostilesInRange.Count; i++)
			{
				IPointOfInterest pointOfInterest = hostilesInRange[i];
				if (!base.owner.marker.IsPOIInVision(pointOfInterest))
				{
					continue;
				}
				avoidInRange.Add(pointOfInterest);
				if (combatDataDictionary.ContainsKey(pointOfInterest))
				{
					combatDataDictionary[pointOfInterest].SetFlightData(reason);
				}
				else
				{
					CombatData combatData = ObjectPoolManager.Instance.CreateNewCombatData();
					combatData.SetFlightData(reason);
					combatDataDictionary.Add(pointOfInterest, combatData);
				}
				if (pointOfInterest is Character)
				{
					Character character = pointOfInterest as Character;
					if (character.combatComponent.combatMode == COMBAT_MODE.Defend)
					{
						character.combatComponent.RemoveHostileInRange(base.owner);
					}
				}
			}
		}
		else
		{
			for (int j = 0; j < hostilesInRange.Count; j++)
			{
				IPointOfInterest pointOfInterest2 = hostilesInRange[j];
				if (pointOfInterest2 is Character)
				{
					Character character2 = pointOfInterest2 as Character;
					if (character2.combatComponent.combatMode == COMBAT_MODE.Defend)
					{
						character2.combatComponent.RemoveHostileInRange(base.owner);
					}
				}
			}
		}
		ClearHostilesInRange(processCombatBehavior: false);
		SetWillProcessCombat(state: true);
	}

	private float GetAttackRange()
	{
		if (base.owner is Dragon)
		{
			return CharacterManager.Instance.GetCharacterClass("Dragon").attackRange;
		}
		return base.owner.characterClass.attackRange;
	}

	public bool RemoveHostileInRange(IPointOfInterest poi, bool processCombatBehavior = true)
	{
		if (hostilesInRange.Remove(poi))
		{
			if (poi is TileObject tileObject)
			{
				tileObject.AdjustRepairCounter(-1);
			}
			else if (poi is Character targetCharacter)
			{
				AddPOIToBannedFromHostile(targetCharacter);
			}
			if (processCombatBehavior)
			{
				if (base.owner.combatComponent.isInCombat)
				{
					CombatState combatState = base.owner.stateComponent.currentState as CombatState;
					if (combatState.forcedTarget == poi)
					{
						combatState.SetForcedTarget(null);
					}
					if (combatState.currentClosestHostile == poi)
					{
						combatState.ResetClosestHostile();
					}
				}
				SetWillProcessCombat(state: true);
			}
			if (poi is TileObject { gridTileLocation: not null } tileObject2 && tileObject2.gridTileLocation.structure is DemonicStructure demonicStructure)
			{
				demonicStructure.RemoveAttacker(base.owner);
			}
			return true;
		}
		return false;
	}

	public void ClearHostilesInRange(bool processCombatBehavior = true)
	{
		if (hostilesInRange.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < hostilesInRange.Count; i++)
		{
			IPointOfInterest pointOfInterest = hostilesInRange[i];
			if (pointOfInterest is TileObject tileObject)
			{
				tileObject.AdjustRepairCounter(-1);
			}
			else if (pointOfInterest is Character targetCharacter)
			{
				AddPOIToBannedFromHostile(targetCharacter);
			}
			if (pointOfInterest is TileObject { gridTileLocation: not null } tileObject2 && tileObject2.gridTileLocation.structure is DemonicStructure demonicStructure)
			{
				demonicStructure.RemoveAttacker(base.owner);
			}
		}
		hostilesInRange.Clear();
		if (processCombatBehavior)
		{
			SetWillProcessCombat(state: true);
		}
	}

	public bool GetLethalityFromCombatData(Character character)
	{
		if (combatDataDictionary.ContainsKey(character))
		{
			return combatDataDictionary[character].isLethal;
		}
		return true;
	}

	public bool ShouldCombatBeLethalAgainst(Character p_targetCharacter)
	{
		bool flag = true;
		if (base.owner.partyComponent.hasParty && base.owner.partyComponent.currentParty.isActive)
		{
			if (base.owner.partyComponent.currentParty.currentQuest is DemonSnatchPartyQuest demonSnatchPartyQuest && demonSnatchPartyQuest.targetCharacter == p_targetCharacter && base.owner.partyComponent.isMemberThatJoinedQuest)
			{
				flag = false;
			}
			if (base.owner.partyComponent.currentParty.currentQuest is DemonRaidPartyQuest demonRaidPartyQuest && p_targetCharacter.homeSettlement == demonRaidPartyQuest.targetSettlement)
			{
				if (base.owner.partyComponent.isActiveMember)
				{
					flag = false;
				}
			}
			else if (base.owner.partyComponent.currentParty.currentQuest is RaidPartyQuest raidPartyQuest && p_targetCharacter.homeSettlement == raidPartyQuest.targetSettlement && base.owner.partyComponent.isActiveMember)
			{
				flag = false;
			}
			if (base.owner.partyComponent.currentParty.currentQuest is DemonStealPartyQuest demonStealPartyQuest && demonStealPartyQuest.targetItem.isBeingCarriedBy == p_targetCharacter)
			{
				if (base.owner.partyComponent.isMemberThatJoinedQuest)
				{
					flag = false;
				}
			}
			else if (base.owner.partyComponent.currentParty.currentQuest is BountyHuntPartyQuest bountyHuntPartyQuest && bountyHuntPartyQuest.targetCharacter == p_targetCharacter && base.owner.partyComponent.isMemberThatJoinedQuest)
			{
				flag = false;
			}
		}
		if (flag)
		{
			if (base.owner.jobQueue.jobsInQueue.Count > 0 && !base.owner.jobQueue.jobsInQueue[0].jobType.IsJobLethal())
			{
				if (base.owner.jobQueue.jobsInQueue.Count > 0 && base.owner.jobQueue.jobsInQueue[0].poiTarget == p_targetCharacter)
				{
					flag = false;
				}
			}
			else
			{
				CombatData combatData = base.owner.combatComponent.GetCombatData(p_targetCharacter);
				if (combatData != null && combatData.connectedAction != null && !combatData.connectedAction.associatedJobType.IsJobLethal())
				{
					flag = false;
				}
			}
		}
		return flag;
	}

	public bool GetCurrentTargetCombatLethality()
	{
		bool result = true;
		if (isInCombat)
		{
			CombatState combatState = base.owner.stateComponent.currentState as CombatState;
			if (combatState.currentClosestHostile != null && combatState.currentClosestHostile is Character character && !GetLethalityFromCombatData(character))
			{
				return false;
			}
		}
		if (hostilesInRange.Count > 0)
		{
			for (int i = 0; i < hostilesInRange.Count; i++)
			{
				if (hostilesInRange[i] is Character character2)
				{
					if (GetLethalityFromCombatData(character2))
					{
						break;
					}
					return false;
				}
			}
		}
		return result;
	}

	public bool HasLethalCombatTarget()
	{
		for (int i = 0; i < hostilesInRange.Count; i++)
		{
			IPointOfInterest pointOfInterest = hostilesInRange[i];
			if (pointOfInterest is Character)
			{
				Character character = pointOfInterest as Character;
				if (GetLethalityFromCombatData(character))
				{
					return true;
				}
			}
		}
		return false;
	}

	public IPointOfInterest GetNearestValidHostile()
	{
		IPointOfInterest pointOfInterest = null;
		float num = 9999f;
		for (int i = 0; i < hostilesInRange.Count; i++)
		{
			IPointOfInterest pointOfInterest2 = hostilesInRange[i];
			if (pointOfInterest2.IsValidCombatTargetFor(base.owner))
			{
				float num2 = Vector2.Distance(base.owner.marker.transform.position, pointOfInterest2.worldPosition);
				if (pointOfInterest == null || num2 < num)
				{
					pointOfInterest = pointOfInterest2;
					num = num2;
				}
			}
			else if (RemoveHostileInRange(pointOfInterest2, processCombatBehavior: false))
			{
				i--;
			}
		}
		if (pointOfInterest == null)
		{
			for (int j = 0; j < hostilesInRange.Count; j++)
			{
				IPointOfInterest pointOfInterest3 = hostilesInRange[j];
				if (pointOfInterest3.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
				{
					if (pointOfInterest3.IsValidCombatTargetFor(base.owner))
					{
						pointOfInterest = hostilesInRange[j];
						break;
					}
					if (RemoveHostileInRange(pointOfInterest3, processCombatBehavior: false))
					{
						j--;
					}
				}
			}
		}
		return pointOfInterest;
	}

	public IPointOfInterest GetNearestValidHostilePriorityNotFleeing()
	{
		IPointOfInterest pointOfInterest = null;
		float num = 9999f;
		for (int i = 0; i < hostilesInRange.Count; i++)
		{
			IPointOfInterest pointOfInterest2 = hostilesInRange[i];
			if (pointOfInterest2.IsValidCombatTargetFor(base.owner))
			{
				if (!(pointOfInterest2 is Character character) || !character.combatComponent.isInCombat || (character.stateComponent.currentState as CombatState).isAttacking)
				{
					float num2 = Vector2.Distance(base.owner.marker.transform.position, pointOfInterest2.worldPosition);
					if (pointOfInterest == null || num2 < num)
					{
						pointOfInterest = pointOfInterest2;
						num = num2;
					}
				}
			}
			else if (RemoveHostileInRange(pointOfInterest2, processCombatBehavior: false))
			{
				i--;
			}
		}
		if (pointOfInterest != null)
		{
			return pointOfInterest;
		}
		return GetNearestValidHostile();
	}

	public IPointOfInterest GetNearestNonResistantValidHostile()
	{
		IPointOfInterest pointOfInterest = null;
		float num = 9999f;
		for (int i = 0; i < hostilesInRange.Count; i++)
		{
			IPointOfInterest pointOfInterest2 = hostilesInRange[i];
			if (pointOfInterest2.IsValidCombatTargetFor(base.owner) && base.owner.marker.IsPOIInVision(pointOfInterest2))
			{
				if (!CombatManager.Instance.IsImmuneToElement(pointOfInterest2, currentElement.type))
				{
					float num2 = Vector2.Distance(base.owner.marker.transform.position, pointOfInterest2.worldPosition);
					if (pointOfInterest == null || num2 < num)
					{
						pointOfInterest = pointOfInterest2;
						num = num2;
					}
				}
			}
			else if (RemoveHostileInRange(pointOfInterest2, processCombatBehavior: false))
			{
				i--;
			}
		}
		if (pointOfInterest == null)
		{
			for (int j = 0; j < hostilesInRange.Count; j++)
			{
				IPointOfInterest pointOfInterest3 = hostilesInRange[j];
				if (pointOfInterest3.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
				{
					if (pointOfInterest3.IsValidCombatTargetFor(base.owner))
					{
						pointOfInterest = hostilesInRange[j];
						break;
					}
					if (RemoveHostileInRange(pointOfInterest3, processCombatBehavior: false))
					{
						j--;
					}
				}
			}
		}
		return pointOfInterest;
	}

	public Character GetNearestValidHostileFromList(List<Character> p_list)
	{
		Character character = null;
		float num = 9999f;
		for (int i = 0; i < p_list.Count; i++)
		{
			Character character2 = p_list[i];
			if (character2.IsValidCombatTargetFor(base.owner))
			{
				float num2 = Vector2.Distance(base.owner.marker.transform.position, character2.worldPosition);
				if (character == null || num2 < num)
				{
					character = character2;
					num = num2;
				}
			}
		}
		return character;
	}

	private void AddPOIToBannedFromHostile(Character targetCharacter)
	{
		if (!base.owner.movementComponent.isStationary && !targetCharacter.isDead && !targetCharacter.traitContainer.HasTrait("Unconscious") && targetCharacter.combatComponent.isInCombat && !(targetCharacter.stateComponent.currentState as CombatState).isAttacking && !bannedFromHostileList.Contains(targetCharacter))
		{
			bannedFromHostileList.Add(targetCharacter);
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(2);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				RemovePOIToBannedFromHostile(targetCharacter);
			}, base.owner);
		}
	}

	private bool RemovePOIToBannedFromHostile(Character targetCharacter)
	{
		return bannedFromHostileList.Remove(targetCharacter);
	}

	public bool HasCharacterInVisionWithSameHostile(Character hostile)
	{
		if ((bool)base.owner.marker)
		{
			for (int i = 0; i < base.owner.marker.inVisionCharacters.Count; i++)
			{
				Character character = base.owner.marker.inVisionCharacters[i];
				if (character != hostile && character.combatComponent.IsHostileInRange(hostile))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsCurrentlyAttackingFriendlyWith(Character p_character)
	{
		if (isInCombat)
		{
			CombatState combatState = base.owner.stateComponent.currentState as CombatState;
			if (combatState.currentClosestHostile != null && combatState.currentClosestHostile is Character { faction: not null } character && p_character.faction != null && character.faction.IsFriendlyWith(p_character.faction))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCurrentlyAttackingDemonicStructure()
	{
		if (isInCombat)
		{
			CombatState combatState = base.owner.stateComponent.currentState as CombatState;
			if (combatState.currentClosestHostile != null && combatState.currentClosestHostile is TileObject tileObject && ((tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure.structureType.IsPlayerStructure()) || tileObject.tileObjectType.IsDemonicStructureTileObject()))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCurrentlyAttackingPartyMateOf(Character p_character)
	{
		if (isInCombat)
		{
			CombatState combatState = base.owner.stateComponent.currentState as CombatState;
			if (combatState.currentClosestHostile != null && combatState.currentClosestHostile is Character character && p_character.partyComponent.hasParty && character.partyComponent.IsAMemberOfParty(p_character.partyComponent.currentParty))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHostileInRange(IPointOfInterest p_poi)
	{
		return hostilesInRange.Contains(p_poi);
	}

	private bool AddAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true, string reason = "")
	{
		if (base.owner.limiterComponent.canMove && !IsAvoidInRange(poi))
		{
			avoidInRange.Add(poi);
			SetWillProcessCombat(state: true);
			return true;
		}
		return false;
	}

	public bool RemoveAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true)
	{
		if (avoidInRange.Remove(poi))
		{
			if (processCombatBehavior)
			{
				SetWillProcessCombat(state: true);
			}
			return true;
		}
		return false;
	}

	public void RemoveAvoidInRangeSchedule(IPointOfInterest poi, bool processCombatBehavior = true)
	{
		if (IsAvoidInRange(poi))
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(3);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				FinalCheckForRemoveAvoidSchedule(poi, processCombatBehavior);
			}, base.owner);
		}
	}

	private void FinalCheckForRemoveAvoidSchedule(IPointOfInterest poi, bool processCombatBehavior)
	{
		if ((bool)base.owner.marker && !base.owner.marker.IsStillInRange(poi))
		{
			RemoveAvoidInRange(poi, processCombatBehavior);
		}
	}

	public void RemoveHostileInRangeSchedule(IPointOfInterest poi, bool processCombatBehavior = true)
	{
		if (IsHostileInRange(poi) && combatDataDictionary.ContainsKey(poi) && combatDataDictionary[poi].reasonForCombat != "Demon Kill" && combatDataDictionary[poi].connectedAction == null)
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(2);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				FinalCheckForRemoveHostileSchedule(poi, processCombatBehavior);
			}, base.owner);
		}
	}

	private void FinalCheckForRemoveHostileSchedule(IPointOfInterest poi, bool processCombatBehavior)
	{
		if ((bool)base.owner.marker && !base.owner.marker.IsStillInRange(poi))
		{
			RemoveHostileInRange(poi, processCombatBehavior);
		}
	}

	public void ClearAvoidInRange(bool processCombatBehavior = true)
	{
		if (avoidInRange.Count > 0)
		{
			avoidInRange.Clear();
			if (processCombatBehavior)
			{
				SetWillProcessCombat(state: true);
			}
		}
	}

	public bool IsAvoidInRange(IPointOfInterest p_poi)
	{
		return avoidInRange.Contains(p_poi);
	}

	public CombatData GetCombatData(IPointOfInterest target)
	{
		if (combatDataDictionary.ContainsKey(target))
		{
			return combatDataDictionary[target];
		}
		return null;
	}

	public void ClearCombatData()
	{
		foreach (CombatData value in combatDataDictionary.Values)
		{
			ObjectPoolManager.Instance.ReturnCombatDataToPool(value);
		}
		combatDataDictionary.Clear();
	}

	public string GetCombatLogKeyReason(IPointOfInterest target)
	{
		string text = string.Empty;
		if (target != null)
		{
			CombatData combatData = GetCombatData(target);
			if (combatData != null)
			{
				text = combatData.reasonForCombat;
				switch (text)
				{
				case "Action":
					text = GetCombatActionReason(combatData.connectedAction, target);
					break;
				case "Anger":
					if (base.owner.traitContainer.HasTrait("Angry") && base.owner.traitContainer.GetTraitOrStatus<Trait>("Angry").IsResponsibleForTrait(target as Character))
					{
						text = "Anger_Target";
					}
					break;
				case "Rage":
					text = "Rage";
					break;
				case "Hostility":
					text = GetHostilityReason(target, combatData);
					break;
				case "Retaliation":
					text = GetRetaliationReason(target, combatData);
					break;
				}
			}
		}
		return text;
	}

	public string GetCombatStateIconString(IPointOfInterest target)
	{
		string result = GoapActionStateDB.Hostile_Icon;
		if (target != null)
		{
			CombatData combatData = GetCombatData(target);
			if (combatData != null && combatData.connectedAction != null)
			{
				result = GetCombatStateIconString(target, combatData.connectedAction);
			}
		}
		return result;
	}

	public string GetCombatStateIconString(IPointOfInterest target, ActualGoapNode action)
	{
		string result = GoapActionStateDB.Hostile_Icon;
		switch (action.associatedJobType)
		{
		case JOB_TYPE.RITUAL_KILLING:
		case JOB_TYPE.MONSTER_ABDUCT:
		case JOB_TYPE.KIDNAP_RAID:
		case JOB_TYPE.CAPTURE_CHARACTER:
			result = GoapActionStateDB.Stealth_Icon;
			break;
		case JOB_TYPE.PRODUCE_FOOD:
		case JOB_TYPE.MONSTER_BUTCHER:
		case JOB_TYPE.PRODUCE_FOOD_FOR_CAMP:
			result = GoapActionStateDB.Butcher_Icon;
			break;
		case JOB_TYPE.RESTRAIN:
		case JOB_TYPE.APPREHEND:
		case JOB_TYPE.APPREHEND_RESTRAINED:
			result = GoapActionStateDB.Restrain_Icon;
			break;
		case JOB_TYPE.DESTROY:
		case JOB_TYPE.BRAWL:
		case JOB_TYPE.BERSERK_ATTACK:
		case JOB_TYPE.ANGRY_DESTROY:
			result = GoapActionStateDB.Anger_Icon;
			break;
		}
		return result;
	}

	public string GetCombatActionReason(ActualGoapNode action, IPointOfInterest target)
	{
		if (action != null)
		{
			switch (action.associatedJobType)
			{
			case JOB_TYPE.RESTRAIN:
				return "Restrain";
			case JOB_TYPE.PRODUCE_FOOD:
			case JOB_TYPE.MONSTER_BUTCHER:
			case JOB_TYPE.PRODUCE_FOOD_FOR_CAMP:
				return "Butcher";
			case JOB_TYPE.APPREHEND:
			case JOB_TYPE.APPREHEND_RESTRAINED:
				return "Apprehend";
			case JOB_TYPE.RITUAL_KILLING:
				return "Ritual Killing";
			case JOB_TYPE.BERSERK_ATTACK:
				return "Berserked";
			case JOB_TYPE.BRAWL:
				return "Snapped";
			case JOB_TYPE.DESTROY:
			case JOB_TYPE.ANGRY_DESTROY:
			{
				string result = "Unknown";
				if (action.otherData != null && action.otherData.Length == 1 && action.otherData[0] is StringOtherData stringOtherData)
				{
					result = stringOtherData.str;
				}
				return result;
			}
			case JOB_TYPE.MONSTER_ABDUCT:
			case JOB_TYPE.CAPTURE_CHARACTER:
				return "Abduct";
			case JOB_TYPE.KIDNAP_RAID:
				return "Raid";
			case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
			case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
			case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:
				return "Fullness_Recovery";
			case JOB_TYPE.SNATCH:
			case JOB_TYPE.SNATCH_RESTRAIN:
				return "Snatch";
			case JOB_TYPE.SLAY_TARGET:
				return "Slay_Target";
			case JOB_TYPE.STALKER_HUNT:
				return "Stalker_Hunt";
			case JOB_TYPE.ASSASSINATE:
				return "Assassination";
			case JOB_TYPE.DEMON_KILL:
				return "Assassination";
			}
		}
		return string.Empty;
	}

	private string GetRetaliationReason(IPointOfInterest target, CombatData combatData)
	{
		if (target is Character character)
		{
			CombatData combatData2 = character.combatComponent.GetCombatData(base.owner);
			if (combatData2 != null)
			{
				string reasonForCombat = combatData2.reasonForCombat;
				if (!string.IsNullOrEmpty(reasonForCombat))
				{
					string text = string.Empty;
					if (reasonForCombat == "Action")
					{
						text = character.combatComponent.GetCombatActionReason(combatData2.connectedAction, base.owner);
					}
					if (text == "Apprehend")
					{
						return "Resisting_Arrest";
					}
					if (text == "Abduct")
					{
						return "Resisting_Abduction";
					}
				}
			}
		}
		return "Defending_Self";
	}

	private string GetHostilityReason(IPointOfInterest target, CombatData combatData)
	{
		Character character = target as Character;
		if (base.owner.partyComponent.isMemberThatJoinedQuest)
		{
			PartyQuest currentQuest = base.owner.partyComponent.currentParty.currentQuest;
			BaseSettlement baseSettlement = null;
			if (currentQuest is RaidPartyQuest raidPartyQuest)
			{
				baseSettlement = raidPartyQuest.targetSettlement;
			}
			else if (currentQuest is DemonRaidPartyQuest demonRaidPartyQuest)
			{
				baseSettlement = demonRaidPartyQuest.targetSettlement;
			}
			if (baseSettlement != null)
			{
				if (character != null)
				{
					if (character.homeSettlement == baseSettlement)
					{
						return "Raid";
					}
				}
				else if (target is TileObject && target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(baseSettlement))
				{
					return "Raid";
				}
			}
		}
		if (character != null)
		{
			if (base.owner.minion != null && character.faction != null && character.faction.isMajorNonPlayer)
			{
				if (combatData.isLethal)
				{
					return "Slaying_Villager";
				}
				return "Incapacitating_Villager";
			}
			if (character.minion != null)
			{
				if (combatData.isLethal)
				{
					return "Slaying_Demon";
				}
				return "Incapacitating_Demon";
			}
			if (character.faction != null)
			{
				if (character.faction.factionType.type == FACTION_TYPE.Vagrants || character.faction.factionType.type == FACTION_TYPE.Retaliator)
				{
					return "Fighting_Vagrant";
				}
				if (character.faction.factionType.type == FACTION_TYPE.Wild_Monsters)
				{
					if (combatData.isLethal)
					{
						return "Slaying_Monster";
					}
					return "Incapacitating_Monster";
				}
				if (character.faction.factionType.type == FACTION_TYPE.Undead)
				{
					if (combatData.isLethal)
					{
						return "Slaying_Undead";
					}
					return "Incapacitating_Undead";
				}
				if (base.owner.faction != null && base.owner.faction.IsHostileWith(character.faction))
				{
					return "Warring_Factions";
				}
			}
		}
		return "Hostility";
	}

	public void OnJobRemovedFromQueue(JobQueueItem job)
	{
		if (job.finishedSuccessfully)
		{
			return;
		}
		bool flag = false;
		foreach (KeyValuePair<IPointOfInterest, CombatData> item in combatDataDictionary)
		{
			if (item.Value.connectedAction != null && item.Value.connectedAction.associatedJob == job && !IsInActualCombatWith(item.Key) && RemoveHostileInRange(item.Key, processCombatBehavior: false))
			{
				flag = true;
			}
		}
		if (flag)
		{
			SetWillProcessCombat(state: true);
		}
	}

	public void UpdateBasicData(bool resetHP)
	{
		UpdateAttack();
		UpdateAttackSpeed();
		if (resetHP)
		{
			UpdateMaxHPAndReset();
		}
		else
		{
			UpdateMaxHPAndProportionateHP();
		}
	}

	public void UpdateAttack()
	{
		int num = ((base.owner.characterClass.attackType == ATTACK_TYPE.PHYSICAL) ? strengthModification : intelligenceModification);
		float num2 = ((base.owner.characterClass.attackType == ATTACK_TYPE.PHYSICAL) ? strengthPercentModification : intelligencePercentModification);
		int num3 = unModifiedAttack + num;
		attack = Mathf.RoundToInt((float)num3 * (num2 / 100f + 1f));
	}

	public float GetDPS()
	{
		return (float)attack / GetAttackSpeedInSeconds();
	}

	public float GetAttackSpeedInSeconds()
	{
		return (float)attackSpeed / 1000f;
	}

	public int GetComputedStrength()
	{
		int num = 0;
		if (base.owner.characterClass.attackType == ATTACK_TYPE.PHYSICAL)
		{
			num = unModifiedAttack;
		}
		return Mathf.RoundToInt((float)(num + strengthModification) * (strengthPercentModification / 100f + 1f));
	}

	public int GetComputedIntelligence()
	{
		int num = 0;
		if (base.owner.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			num = unModifiedAttack;
		}
		return Mathf.RoundToInt((float)(num + intelligenceModification) * (intelligencePercentModification / 100f + 1f));
	}

	private void UpdateMaxHP()
	{
		int num = unModifiedMaxHP + maxHPModification;
		maxHP = Mathf.RoundToInt((float)num * (maxHPPercentModification / 100f + 1f));
		if (maxHP < 0)
		{
			maxHP = 1;
		}
	}

	private void UpdateAttackSpeed()
	{
		int baseAttackSpeed = base.owner.characterClass.baseAttackSpeed;
		float num = attackSpeedPercentModification;
		attackSpeed = Mathf.RoundToInt((float)baseAttackSpeed * (num / 100f + 1f));
	}

	public void UpdateMaxHPAndReset()
	{
		UpdateMaxHP();
		base.owner.ResetToFullHP();
	}

	public void UpdateMaxHPAndProportionateHP()
	{
		float num = (float)base.owner.currentHP / (float)maxHP;
		UpdateMaxHP();
		int num2 = Mathf.RoundToInt(num * (float)maxHP);
		if (num2 < 0)
		{
			num2 = 0;
		}
		base.owner.SetHP(num2);
	}

	public void AdjustMaxHPModifier(int modification)
	{
		maxHPModification += modification;
		UpdateMaxHPAndProportionateHP();
	}

	public void AdjustMaxHPPercentModifier(float modification)
	{
		maxHPPercentModification += modification;
		UpdateMaxHPAndProportionateHP();
	}

	public void AdjustAttackModifier(int modification)
	{
		strengthModification += modification;
		intelligenceModification += modification;
		UpdateAttack();
	}

	public void AdjustAttackPercentModifier(float modification)
	{
		strengthPercentModification += modification;
		intelligencePercentModification += modification;
		UpdateAttack();
	}

	public void AdjustStrengthModifier(int modification)
	{
		strengthModification += modification;
		UpdateAttack();
	}

	public void AdjustStrengthPercentModifier(float modification)
	{
		strengthPercentModification += modification;
		UpdateAttack();
	}

	public void AdjustIntelligenceModifier(int modification)
	{
		intelligenceModification += modification;
		UpdateAttack();
	}

	public void AdjustIntelligencePercentModifier(float modification)
	{
		intelligencePercentModification += modification;
		UpdateAttack();
	}

	public void AdjustCritRate(int modification)
	{
		critRate += modification;
	}

	public void AdjustAttackSpeedPercentModifier(float modification)
	{
		attackSpeedPercentModification += modification;
		UpdateAttackSpeed();
	}

	private void OnCharacterBecomePrisoner(Prisoner prisoner)
	{
		if (prisoner.IsConsideredPrisonerOf(base.owner))
		{
			CombatData combatData = GetCombatData(prisoner.owner);
			if (combatData != null && (combatData.reasonForCombat == "Hostility" || combatData.reasonForCombat == "Retaliation"))
			{
				RemoveHostileInRange(prisoner.owner);
			}
		}
	}

	public void AdjustNumOfKilledCharacters(int amount)
	{
		numOfKilledCharacters += amount;
	}

	public bool IsUnkillable(Character p_character)
	{
		return unkillableCharacters.Contains(p_character);
	}

	public void AddUnkillableCharacter(Character p_character)
	{
		if (!IsUnkillable(p_character))
		{
			unkillableCharacters.Add(p_character);
			ResetClearUnkillableListTicks();
			SetShouldIncreaseClearUnkillableListTicks(p_state: true);
		}
	}

	private void ClearUnkillableCharacters()
	{
		unkillableCharacters.Clear();
	}

	private void IncreaseClearUnkillableListTicks()
	{
		if (shouldIncreaseClearUnkillableListTicks)
		{
			clearUnkillableListTicks++;
			if (clearUnkillableListTicks >= CharacterManager.Instance.CHARACTER_CLEAR_UNKILLABLE_LIST_TICKS)
			{
				ClearUnkillableCharacters();
				ResetClearUnkillableListTicks();
				SetShouldIncreaseClearUnkillableListTicks(p_state: false);
			}
		}
	}

	private void ResetClearUnkillableListTicks()
	{
		clearUnkillableListTicks = 0;
	}

	private void SetShouldIncreaseClearUnkillableListTicks(bool p_state)
	{
		shouldIncreaseClearUnkillableListTicks = p_state;
	}

	public void AdjustHPRecoveryPerTickOutsideCombat(int p_amount)
	{
		hpRecoveryPerTickOutsideCombat += p_amount;
	}

	public bool ExecuteSpecialSkill(IPointOfInterest p_target)
	{
		if (!specialSkillParent.HasSpecialSkill())
		{
			return false;
		}
		if (!base.owner.marker.CanAttackByAttackSpeed())
		{
			return false;
		}
		if (base.owner.combatComponent.specialSkillParent.TryActivateSpecialSkill(base.owner))
		{
			if (base.owner.marker.isMoving)
			{
				base.owner.marker.StopMovement();
			}
			InnerMapManager.Instance.FaceTarget(base.owner, p_target);
			base.owner.marker.ResetAttackSpeed();
			return true;
		}
		return false;
	}

	public void LoadReferences(SaveDataCombatComponent data)
	{
		for (int i = 0; i < data.hostileCharactersInRange.Count; i++)
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(data.hostileCharactersInRange[i]);
			if (characterByPersistentID != null && !IsHostileInRange(characterByPersistentID))
			{
				hostilesInRange.Add(characterByPersistentID);
			}
		}
		for (int j = 0; j < data.hostileTileObjectsInRange.Count; j++)
		{
			TileObject tileObjectByPersistentIDSafe = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentIDSafe(data.hostileTileObjectsInRange[j]);
			if (tileObjectByPersistentIDSafe != null && !IsHostileInRange(tileObjectByPersistentIDSafe))
			{
				hostilesInRange.Add(tileObjectByPersistentIDSafe);
			}
		}
		for (int k = 0; k < data.avoidCharactersInRange.Count; k++)
		{
			Character characterByPersistentID2 = CharacterManager.Instance.GetCharacterByPersistentID(data.avoidCharactersInRange[k]);
			if (characterByPersistentID2 != null && !IsAvoidInRange(characterByPersistentID2))
			{
				avoidInRange.Add(characterByPersistentID2);
			}
		}
		for (int l = 0; l < data.avoidTileObjectsInRange.Count; l++)
		{
			TileObject tileObjectByPersistentIDSafe2 = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentIDSafe(data.avoidTileObjectsInRange[l]);
			if (tileObjectByPersistentIDSafe2 != null && !IsAvoidInRange(tileObjectByPersistentIDSafe2))
			{
				avoidInRange.Add(tileObjectByPersistentIDSafe2);
			}
		}
		foreach (KeyValuePair<string, SaveDataCombatData> characterCombatDatum in data.characterCombatData)
		{
			Character characterByPersistentID3 = CharacterManager.Instance.GetCharacterByPersistentID(characterCombatDatum.Key);
			if (characterByPersistentID3 != null)
			{
				CombatData value = characterCombatDatum.Value.Load();
				combatDataDictionary.Add(characterByPersistentID3, value);
			}
		}
		foreach (KeyValuePair<string, SaveDataCombatData> tileObjectCombatDatum in data.tileObjectCombatData)
		{
			TileObject tileObjectByPersistentIDSafe3 = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentIDSafe(tileObjectCombatDatum.Key);
			if (tileObjectByPersistentIDSafe3 != null)
			{
				CombatData value2 = tileObjectCombatDatum.Value.Load();
				combatDataDictionary.Add(tileObjectByPersistentIDSafe3, value2);
			}
		}
		if (data.bannedFromHostileList != null)
		{
			for (int m = 0; m < data.bannedFromHostileList.Count; m++)
			{
				Character characterByPersistentID4 = CharacterManager.Instance.GetCharacterByPersistentID(data.bannedFromHostileList[m]);
				if (characterByPersistentID4 != null && !bannedFromHostileList.Contains(characterByPersistentID4))
				{
					bannedFromHostileList.Add(characterByPersistentID4);
				}
			}
		}
		if (data.unkillableCharacters != null)
		{
			for (int n = 0; n < data.unkillableCharacters.Count; n++)
			{
				Character characterByPersistentID5 = CharacterManager.Instance.GetCharacterByPersistentID(data.unkillableCharacters[n]);
				if (characterByPersistentID5 != null && !unkillableCharacters.Contains(characterByPersistentID5))
				{
					unkillableCharacters.Add(characterByPersistentID5);
				}
			}
		}
		elementalStatusWaitingList = new List<ELEMENTAL_TYPE>(10);
		elementalStatusWaitingList.AddRange(data.elementalStatusWaitingList);
		combatBehaviourParent.LoadReferences(data.combatBehaviourParent);
		specialSkillParent.LoadReferences();
	}

	public override void CleanUp()
	{
		base.CleanUp();
		hostilesInRange?.Clear();
		hostilesInRange = null;
		avoidInRange?.Clear();
		avoidInRange = null;
		bannedFromHostileList?.Clear();
		bannedFromHostileList = null;
		unkillableCharacters?.Clear();
		unkillableCharacters = null;
		combatDataDictionary?.Clear();
		combatDataDictionary = null;
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		RemoveHostileInRange(p_character, processCombatBehavior: false);
		RemoveAvoidInRange(p_character, processCombatBehavior: false);
		RemovePOIToBannedFromHostile(p_character);
		unkillableCharacters.Remove(p_character);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		hostilesInRange.Contains(p_character);
		avoidInRange.Contains(p_character);
		bannedFromHostileList.Contains(p_character);
		unkillableCharacters.Contains(p_character);
	}
}
