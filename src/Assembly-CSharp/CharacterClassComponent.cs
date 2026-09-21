using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CharacterClassComponent : CharacterComponent
{
	public CharacterClass characterClass { get; private set; }

	public string previousClassName { get; private set; }

	public List<string> ableClasses { get; private set; }

	public bool shouldChangeClass { get; private set; }

	public int devastationCounter { get; private set; }

	public CHARACTER_CATEGORY hunterKillingSpecialization { get; private set; }

	public StalkerBonuses stalkerBonuses { get; private set; }

	public bool canChangeClass => !characterClass.IsSpecialClass();

	public CharacterClassComponent()
	{
		previousClassName = string.Empty;
		hunterKillingSpecialization = CHARACTER_CATEGORY.None;
		ableClasses = new List<string>();
		stalkerBonuses = new StalkerBonuses();
	}

	public CharacterClassComponent(SaveDataCharacterClassComponent data)
	{
		characterClass = CharacterManager.Instance.GetCharacterClass(data.className);
		previousClassName = data.previousClassName;
		shouldChangeClass = data.shouldChangeClass;
		if (data.ableClasses != null && data.ableClasses.Count > 0)
		{
			ableClasses = new List<string>(data.ableClasses);
		}
		else
		{
			ableClasses = new List<string>();
		}
		devastationCounter = data.devastationCounter;
		hunterKillingSpecialization = data.hunterKillingSpecialization;
		stalkerBonuses = new StalkerBonuses(data.stalkerBonuses);
	}

	public void AssignClass(string className, bool isInitial = false)
	{
		if (characterClass == null || className != characterClass.className)
		{
			if (!CharacterManager.Instance.HasCharacterClass(className))
			{
				throw new Exception("There is no class named " + className + " but it is being assigned to " + base.owner.name);
			}
			AssignClass(CharacterManager.Instance.GetCharacterClass(className), isInitial);
		}
	}

	public void AssignClass(CharacterClass p_newClass, bool isInitial = false)
	{
		CharacterClass characterClass = this.characterClass;
		if (characterClass != null)
		{
			if (!isInitial)
			{
				previousClassName = characterClass.className;
			}
			for (int i = 0; i < characterClass.traitNames.Length; i++)
			{
				base.owner.traitContainer.RemoveTrait(base.owner, characterClass.traitNames[i]);
			}
			if (characterClass.IsReligiousCultLeaderClass(out var p_religion))
			{
				CharacterManager.Instance.DecreaseActiveReligiousCultLeaders(p_religion);
			}
		}
		this.characterClass = p_newClass;
		base.owner.movementComponent.OnAssignedClass(p_newClass);
		if (isInitial)
		{
			if (GameManager.Instance.gameHasStarted)
			{
				base.owner.RecomputeResistanceInitialChangeClass(base.owner, "Farmer");
			}
			else
			{
				base.owner.RecomputePiercingAndResistanceForGameStart(base.owner, base.owner.characterClass.className);
			}
		}
		else
		{
			OnUpdateCharacterClass();
			Messenger.Broadcast(CharacterSignals.CHARACTER_CLASS_CHANGE, base.owner, characterClass, this.characterClass);
		}
		if (this.characterClass.IsReligiousCultLeaderClass(out var p_religion2))
		{
			CharacterManager.Instance.IncreaseActiveReligiousCultLeader(p_religion2);
		}
		base.owner.combatComponent.UpdateElementalType();
		if (!base.owner.isDead)
		{
			if (this.characterClass.IsReligiousCultLeaderClass(RELIGION.Demon_Worship))
			{
				PlayerManager.Instance?.player?.playerSkillComponent.GetPrismEvent<CultLeaderEvent>().AdjustNumberOfAliveCultLeaders(1);
			}
			else if (characterClass != null && characterClass.IsReligiousCultLeaderClass(RELIGION.Demon_Worship))
			{
				PlayerManager.Instance?.player?.playerSkillComponent.GetPrismEvent<CultLeaderEvent>().AdjustNumberOfAliveCultLeaders(-1);
			}
		}
	}

	public void OnUpdateCharacterClass()
	{
		CharacterClass characterClass = this.characterClass;
		if (characterClass != null)
		{
			base.owner.combatComponent.combatBehaviourParent.SetCombatBehaviour(characterClass.combatBehaviourType, base.owner);
			base.owner.combatComponent.specialSkillParent.SetSpecialSkill(characterClass.combatSpecialSkillType);
		}
		for (int i = 0; i < this.characterClass.traitNames.Length; i++)
		{
			base.owner.traitContainer.AddTrait(base.owner, this.characterClass.traitNames[i]);
		}
		base.owner.combatComponent.UpdateBasicData(resetHP: false);
		base.owner.needsComponent.UpdateBaseStaminaDecreaseRate();
		base.owner.visuals.UpdateAllVisuals(base.owner);
		base.owner.UpdateCanCombatState();
		if (previousClassName == "Ratman" || previousClassName == "Miner")
		{
			base.owner.movementComponent.SetEnableDigging(state: false);
		}
		if (this.characterClass.className == "Ratman" || this.characterClass.className == "Miner")
		{
			base.owner.movementComponent.SetEnableDigging(state: true);
		}
		if (previousClassName == "Necromancer" && this.characterClass.className != "Werewolf")
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Necromancer");
		}
		if (this.characterClass.className == "Necromancer" && !base.owner.traitContainer.HasTrait("Necromancer"))
		{
			base.owner.traitContainer.AddTrait(base.owner, "Necromancer");
		}
		if (this.characterClass.className == "Hero")
		{
			base.owner.traitContainer.RemoveAllTraitsByType(base.owner, TRAIT_TYPE.FLAW);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "became_hero", LOG_TAG.Major);
			log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
			LogPool.Release(log);
			base.owner.traitContainer.AddTrait(base.owner, "Blessed");
		}
		if (this.characterClass.className == "Mage")
		{
			ResetDevastationCounter();
		}
		if (this.characterClass.IsZombie())
		{
			base.owner.traitContainer.RemoveReligiousCultistTrait(base.owner);
		}
		base.owner.combatComponent.UpdateAttack();
		base.owner.combatComponent.UpdateMaxHPAndProportionateHP();
		if (base.owner.talentComponent != null)
		{
			base.owner.talentComponent.ReevaluateAllTalents();
		}
		if (base.owner.isNormalCharacter && !this.characterClass.IsCombatant() && base.owner.partyComponent.hasParty)
		{
			base.owner.partyComponent.currentParty.RemoveMember(base.owner);
		}
		base.owner.structureComponent.ProcessUnclaimWorkStructure();
		if (this.characterClass.className != "Werewolf")
		{
			base.owner.equipmentComponent.RemoveAllIncompatibleEquipment(base.owner);
		}
	}

	public void OverridePreviousClassName(string p_className)
	{
		previousClassName = p_className;
	}

	public void SetShouldChangeClass(bool p_state)
	{
		shouldChangeClass = p_state;
	}

	public bool AddAbleClass(string p_className)
	{
		if (!HasAbleClass(p_className))
		{
			ableClasses.Add(p_className);
			return true;
		}
		return false;
	}

	public bool RemoveAbleClass(string p_className)
	{
		return ableClasses.Remove(p_className);
	}

	public bool HasAbleClass(string p_className)
	{
		return ableClasses.Contains(p_className);
	}

	public bool HasAbleCombatantClass()
	{
		for (int i = 0; i < ableClasses.Count; i++)
		{
			string p_className = ableClasses[i];
			if (CharacterManager.Instance.GetCharacterClass(p_className).IsCombatant())
			{
				return true;
			}
		}
		return false;
	}

	public string GetAbleClassesText()
	{
		string text = string.Empty;
		for (int i = 0; i < ableClasses.Count; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += ableClasses[i];
		}
		return text;
	}

	public void PopulateAbleCombatantClasses(List<string> p_classes)
	{
		for (int i = 0; i < ableClasses.Count; i++)
		{
			string text = ableClasses[i];
			CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(text);
			if (characterClass.IsCombatant() && (!(characterClass.className == "Stalker") || !base.owner.traitContainer.HasTrait("Demon Cultist")))
			{
				p_classes.Add(text);
			}
		}
	}

	public void PopulateAbleFoodProducerClasses(List<string> p_classes)
	{
		for (int i = 0; i < ableClasses.Count; i++)
		{
			string text = ableClasses[i];
			if (CharacterManager.Instance.GetCharacterClass(text).className.IsFoodProducerClassName())
			{
				p_classes.Add(text);
			}
		}
	}

	public void PopulateBasicProducerClasses(List<string> p_classes, FACTION_TYPE p_factionType)
	{
		for (int i = 0; i < ableClasses.Count; i++)
		{
			string text = ableClasses[i];
			if (CharacterManager.Instance.GetCharacterClass(text).IsBasicResourceProducer(p_factionType))
			{
				p_classes.Add(text);
			}
		}
	}

	public void PopulateAbleSpecialCivilianClasses(List<string> p_classes)
	{
		for (int i = 0; i < ableClasses.Count; i++)
		{
			string text = ableClasses[i];
			if (CharacterManager.Instance.GetCharacterClass(text).className.IsSpecialCivilianClassName())
			{
				p_classes.Add(text);
			}
		}
	}

	public void RandomizeCurrentClassBasedOnAbleClasses()
	{
		string randomElement = CollectionUtilities.GetRandomElement(ableClasses);
		AssignClass(randomElement, isInitial: true);
		OnUpdateCharacterClass();
	}

	public string GetRandomHighestAbleCombatantClass()
	{
		string result = string.Empty;
		List<string> list = RuinarchListPool<string>.Claim();
		base.owner.talentComponent.PopulateHighestAbleCombatantClasses(list);
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		else
		{
			PopulateAbleCombatantClasses(list);
			if (list.Count > 0)
			{
				result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
		}
		RuinarchListPool<string>.Release(list);
		return result;
	}

	public void PerHour()
	{
		if (characterClass.className == "Barbarian")
		{
			if (!ChanceData.RollChance(CHANCE_TYPE.Barbarian_Escape))
			{
				return;
			}
			int num = 0;
			if (base.owner.traitContainer.HasTrait("Restrained"))
			{
				num++;
			}
			else if (base.owner.traitContainer.HasTrait("Ensnared"))
			{
				num++;
			}
			bool flag = base.owner.limiterComponent.canPerformValue + num >= 0;
			if (num > 0 && flag)
			{
				LocationStructure currentStructure = base.owner.currentStructure;
				Log log;
				if (currentStructure != null && currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS)
				{
					currentStructure.AdjustHP(-currentStructure.currentHP);
					log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Barbarian_Escape_Prison", LOG_TAG.Major);
					log.AddToFillers(currentStructure, currentStructure.GetNameRelativeTo(base.owner), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
				}
				else
				{
					log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Barbarian_Escape", LOG_TAG.Major);
				}
				log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
				LogPool.Release(log);
				base.owner.traitContainer.RemoveTrait(base.owner, "Ensnared");
				base.owner.traitContainer.RemoveRestrainAndImprison(base.owner);
				for (int i = 0; i < 5; i++)
				{
					base.owner.traitContainer.AddTrait(base.owner, "Enraged");
				}
			}
		}
		else
		{
			if ((!base.owner.race.IsSapient() && base.owner.race != RACE.RATMAN) || !ChanceData.RollChance(CHANCE_TYPE.Sapient_Escape))
			{
				return;
			}
			int num2 = 0;
			if (base.owner.traitContainer.HasTrait("Restrained"))
			{
				num2++;
			}
			else if (base.owner.traitContainer.HasTrait("Ensnared"))
			{
				num2++;
			}
			bool flag2 = base.owner.limiterComponent.canPerformValue + num2 >= 0;
			if (num2 > 0 && flag2)
			{
				LocationStructure currentStructure2 = base.owner.currentStructure;
				if (currentStructure2 == null || currentStructure2.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS)
				{
					Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Sapient_Escape_Restraints", LOG_TAG.Major);
					log2.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log2.AddLogToDatabase();
					PlayerManager.Instance.player.ShowNotificationFromPlayer(log2);
					LogPool.Release(log2);
					base.owner.traitContainer.RemoveTrait(base.owner, "Ensnared");
					base.owner.traitContainer.RemoveRestrainAndImprison(base.owner);
				}
			}
		}
	}

	public int GetFoodSupplyCapacityValue()
	{
		int num = 0;
		Character character = base.owner;
		if (character.structureComponent.HasWorkPlaceStructure() && character.structureComponent.workPlaceStructure.structureType.IsFoodProducingStructure())
		{
			num += GetFoodSupplyCapacityValueBase();
		}
		return num;
	}

	public int GetFoodSupplyCapacityValueBase()
	{
		int num = 0;
		Character character = base.owner;
		if (!character.isDead && !character.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined"))
		{
			num += 8;
		}
		return num;
	}

	public int GetResourceSupplyCapacityValue(STRUCTURE_TYPE p_structureType)
	{
		int num = 0;
		Character character = base.owner;
		if (character.structureComponent.HasWorkPlaceStructure() && character.structureComponent.workPlaceStructure.structureType == p_structureType)
		{
			num += GetResourceSupplyCapacityValueBase();
		}
		return num;
	}

	public int GetResourceSupplyCapacityValueBase()
	{
		int num = 0;
		Character character = base.owner;
		if (!character.isDead && !character.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined"))
		{
			num += 8;
		}
		return num;
	}

	public int GetCombatSupplyValue()
	{
		int num = 0;
		Character character = base.owner;
		if (!character.isDead && !character.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && character.HasTalents())
		{
			num = ((characterClass.attackType != ATTACK_TYPE.PHYSICAL) ? (num + character.TryGetTalentLevel(CHARACTER_TALENT.Combat_Magic)) : (num + character.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts)));
		}
		return num;
	}

	public void OnCharacterHitByPlayerSpell(int p_amount)
	{
		if (characterClass.className == "Mage")
		{
			AddDevastationCounter(3);
		}
	}

	public void OnCharacterStartedCombatWith(Character p_responsibleCharacter)
	{
		if (p_responsibleCharacter != null && p_responsibleCharacter.faction != null && p_responsibleCharacter.faction.isPlayerFaction && characterClass.className == "Mage")
		{
			AddDevastationCounter(1);
		}
	}

	public void OnCharacterRestrainedBy(Character p_responsibleCharacter)
	{
		if (p_responsibleCharacter != null && p_responsibleCharacter.faction != null && p_responsibleCharacter.faction.isPlayerFaction && characterClass.className == "Mage")
		{
			AddDevastationCounter(5);
		}
	}

	public void OnCharacterStartedBeingTortured()
	{
		if (characterClass.className == "Mage")
		{
			AddDevastationCounter(10);
		}
	}

	public void AddDevastationCounter(int p_amount)
	{
		devastationCounter += p_amount;
		devastationCounter = Mathf.Clamp(devastationCounter, 0, 100);
	}

	public void ResetDevastationCounter()
	{
		devastationCounter = 0;
	}

	public void SetHunterKillingSpecialization(CHARACTER_CATEGORY p_category)
	{
		hunterKillingSpecialization = p_category;
	}

	public void ProcessHittingHunterByHisHunterSpecialization(Character characterThatAttacked, ref int attackPower)
	{
		if (hunterKillingSpecialization != CHARACTER_CATEGORY.None && characterClass.className == "Hunter" && RaceManager.Instance.GetRaceData(characterThatAttacked.race).category == hunterKillingSpecialization)
		{
			attackPower = Mathf.RoundToInt((float)attackPower * 0.5f);
		}
	}

	public bool WillAttackBeACriticalStrikeBasedOnHunterSpecialization(IPointOfInterest p_hitPOI)
	{
		if (p_hitPOI != null && p_hitPOI is Character character && hunterKillingSpecialization != CHARACTER_CATEGORY.None && characterClass.className == "Hunter" && RaceManager.Instance.GetRaceData(character.race).category == hunterKillingSpecialization)
		{
			return true;
		}
		return false;
	}

	public void SetStalkerCannotBeTurned(bool p_state)
	{
		stalkerBonuses.cannotBeTurned = p_state;
	}

	public void SetStalkerDealDoubleDamage(bool p_state)
	{
		stalkerBonuses.dealDoubleDamage = p_state;
	}

	public void SetStalkerCanIdentifyVampiresAndLycans(bool p_state)
	{
		stalkerBonuses.canIdentifyVampireAndLycans = p_state;
	}

	public void SetStalkerCanIdentifyCultists(bool p_state)
	{
		stalkerBonuses.canIdentifyCultists = p_state;
	}

	public bool IsStalkerCannotBeTurned()
	{
		if (characterClass.className == "Stalker")
		{
			return stalkerBonuses.cannotBeTurned;
		}
		return false;
	}

	public bool DoesStalkerDealDoubleDamage()
	{
		if (characterClass.className == "Stalker")
		{
			return stalkerBonuses.dealDoubleDamage;
		}
		return false;
	}

	public bool CanStalkerIdentifyVampiresAndLycans()
	{
		if (characterClass.className == "Stalker")
		{
			return stalkerBonuses.canIdentifyVampireAndLycans;
		}
		return false;
	}

	public bool CanStalkerIdentifyCultists()
	{
		if (characterClass.className == "Stalker")
		{
			return stalkerBonuses.canIdentifyCultists;
		}
		return false;
	}

	public bool IsTargetRecognizedAsVampireByStalker(Character p_target)
	{
		if (characterClass.className == "Stalker")
		{
			if (p_target.traitContainer.HasTrait("Vampire") && p_target.traitContainer.GetTraitOrStatus<Vampire>("Vampire").awareCharacters.Contains(base.owner))
			{
				return true;
			}
			if (!p_target.crimeComponent.HasCrime(CRIME_TYPE.Vampire))
			{
				if (base.owner.classComponent.CanStalkerIdentifyVampiresAndLycans())
				{
					return p_target.traitContainer.HasTrait("Vampire");
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsTargetRecognizedAsLycanByStalker(Character p_target)
	{
		if (characterClass.className == "Stalker")
		{
			if (p_target.isLycanthrope && p_target.lycanData.awareCharacters.Contains(base.owner))
			{
				return true;
			}
			if (!p_target.crimeComponent.HasCrime(CRIME_TYPE.Werewolf) && (!base.owner.classComponent.CanStalkerIdentifyVampiresAndLycans() || !p_target.isLycanthrope || p_target is Summon))
			{
				return p_target.isInWerewolfForm;
			}
			return true;
		}
		return false;
	}

	public bool IsTargetRecognizedAsCultistByStalker(Character p_target)
	{
		if (characterClass.className == "Stalker")
		{
			if (!p_target.crimeComponent.HasCrime(CRIME_TYPE.Demon_Worship, CRIME_TYPE.Divine_Worship, CRIME_TYPE.Nature_Worship))
			{
				if (base.owner.classComponent.CanStalkerIdentifyCultists())
				{
					return p_target.traitContainer.IsReligiousCultist();
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void StalkerHunt(Character p_target)
	{
		if (base.owner.characterClass.className == "Stalker" && base.owner.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) < 5 && !base.owner.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.STALKER_HUNT))
		{
			base.owner.jobComponent.TryCreateStalkerHuntJob(p_target);
		}
	}

	public void LoadReferences(SaveDataCharacterClassComponent data)
	{
		if (characterClass.IsReligiousCultLeaderClass(out var p_religion))
		{
			CharacterManager.Instance.IncreaseActiveReligiousCultLeader(p_religion);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
