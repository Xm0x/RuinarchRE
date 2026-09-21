using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class PlayerUnderlingsComponent
{
	public readonly int cooldown;

	public Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> monsterUnderlingCharges { get; private set; }

	public Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges> demonUnderlingCharges { get; private set; }

	public Party persistentDefendParty { get; private set; }

	public PlayerUnderlingsComponent()
	{
		float cooldownSpeedModification = WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCooldownSpeedModification();
		cooldown = Mathf.CeilToInt((float)GameManager.Instance.GetTicksBasedOnHour(6) * cooldownSpeedModification);
		monsterUnderlingCharges = new Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges>();
		demonUnderlingCharges = new Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges>();
	}

	public PlayerUnderlingsComponent(SaveDataPlayerUnderlingsComponent data)
	{
		cooldown = data.cooldown;
		monsterUnderlingCharges = data.monsterUnderlingCharges;
		demonUnderlingCharges = data.demonUnderlingCharges;
	}

	public void OnCharacterAddedToPlayerFaction(Character p_character)
	{
		if (p_character is Summon arg)
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_SUMMON, arg);
		}
	}

	public void OnCharacterRemovedFromPlayerFaction(Character p_character)
	{
		if (p_character is Summon summon)
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_LOST_SUMMON, summon);
			PlayerManager.Instance.player.underlingsComponent.DecreaseMonsterUnderlingCharge(summon.summonType);
		}
		if (p_character.minion != null)
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_LOST_MINION, p_character.minion);
		}
	}

	public void OnFactionMemberDied(Character character)
	{
		if (character is Summon summon)
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_LOST_SUMMON, summon);
			if (HasMonsterUnderlingEntry(summon.summonType, out var p_monsterAndDemonUnderlingCharges) && p_monsterAndDemonUnderlingCharges.ShouldStartMonsterReplenish())
			{
				p_monsterAndDemonUnderlingCharges.StartMonsterReplenish();
			}
		}
	}

	public void SubscribeListeners()
	{
		Messenger.AddListener<Minion>(PlayerSkillSignals.SUMMON_MINION, OnSummonMinion);
		Messenger.AddListener<Minion>(PlayerSkillSignals.UNSUMMON_MINION, OnUnsummonMinion);
		Messenger.AddListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnSkillChargesAdjusted);
		Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.ADDED_PLAYER_MINION_SKILL, OnGainPlayerMinionSkill);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
	}

	private void OnSpellCooldownFinished(SkillData data)
	{
		if (data is MinionPlayerSkill minionPlayerSkill && WorldSettings.Instance.worldSettingsData.playerSkillSettings.chargeAmount == SKILL_CHARGE_AMOUNT.Unlimited)
		{
			TrySpawnMissingDemons(minionPlayerSkill.minionType);
		}
	}

	private void OnSkillChargesAdjusted(SkillData data, int p_amount)
	{
		if (data is MinionPlayerSkill minionPlayerSkill)
		{
			AdjustDemonUnderlingCharges(minionPlayerSkill.minionType, p_amount, minionPlayerSkill.maxCharges);
		}
	}

	private void OnSummonMinion(Minion minion)
	{
		Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_MINION, minion);
	}

	private void OnUnsummonMinion(Minion minion)
	{
		Messenger.Broadcast(PlayerSignals.PLAYER_LOST_MINION, minion);
	}

	private void OnGainPlayerMinionSkill(PLAYER_SKILL_TYPE p_skillType)
	{
		MinionPlayerSkill minionPlayerSkillData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(p_skillType);
		AddDemonUnderlingEntry(minionPlayerSkillData.minionType, minionPlayerSkillData.charges, minionPlayerSkillData.maxCharges, CharacterManager.Instance.GetMinionSettings(minionPlayerSkillData.minionType).className);
	}

	private void AddDemonUnderlingEntry(MINION_TYPE p_demonType, int currentCharges, int maxCharges, string p_characterClassName)
	{
		if (!HasDemonUnderlingEntry(p_demonType))
		{
			MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges = new MonsterAndDemonUnderlingCharges(p_demonType, currentCharges, maxCharges, p_characterClassName);
			demonUnderlingCharges.Add(p_demonType, monsterAndDemonUnderlingCharges);
			Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, monsterAndDemonUnderlingCharges);
			if (currentCharges > 0)
			{
				SpawnDemonUnderlings(monsterAndDemonUnderlingCharges);
			}
		}
	}

	private bool HasDemonUnderlingEntry(MINION_TYPE p_demonType)
	{
		return demonUnderlingCharges.ContainsKey(p_demonType);
	}

	private void AdjustDemonUnderlingCharges(MINION_TYPE p_demonType, int p_chargeAdjustment, int maxCharge)
	{
		if (HasDemonUnderlingEntry(p_demonType))
		{
			MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges = demonUnderlingCharges[p_demonType];
			monsterAndDemonUnderlingCharges.currentCharges += p_chargeAdjustment;
			monsterAndDemonUnderlingCharges.maxCharges = maxCharge;
			Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, monsterAndDemonUnderlingCharges);
			if (p_chargeAdjustment > 0)
			{
				SpawnDemonUnderlings(monsterAndDemonUnderlingCharges);
			}
		}
	}

	private void AddMonsterUnderlingEntry(SUMMON_TYPE p_monsterType, int currentCharges, int maxCharges, string p_characterClassName)
	{
		if (!HasMonsterUnderlingEntry(p_monsterType))
		{
			MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges = new MonsterAndDemonUnderlingCharges(p_monsterType, currentCharges, maxCharges, p_characterClassName);
			monsterUnderlingCharges.Add(p_monsterType, monsterAndDemonUnderlingCharges);
			Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, monsterAndDemonUnderlingCharges);
			if (currentCharges > 0)
			{
				SpawnMonsterUnderlings(monsterAndDemonUnderlingCharges, currentCharges);
			}
		}
	}

	public void GainMonsterUnderlingMaxChargesFromKennel(SUMMON_TYPE summonType, int amount)
	{
		AdjustMonsterUnderlingMaxCharge(summonType, amount, adjustCurrentCharges: false);
		if (HasMonsterUnderlingEntry(summonType, out var p_monsterAndDemonUnderlingCharges) && p_monsterAndDemonUnderlingCharges.ShouldStartMonsterReplenish())
		{
			p_monsterAndDemonUnderlingCharges.StartMonsterReplenish();
		}
	}

	public void LoseMonsterUnderlingMaxChargesFromKennel(SUMMON_TYPE summonType, int amount)
	{
		AdjustMonsterUnderlingMaxCharge(summonType, amount, adjustCurrentCharges: false);
	}

	public bool HasMonsterUnderlingEntry(SUMMON_TYPE p_monsterType)
	{
		return monsterUnderlingCharges.ContainsKey(p_monsterType);
	}

	public bool HasMonsterUnderlingEntry(SUMMON_TYPE p_monsterType, out MonsterAndDemonUnderlingCharges p_monsterAndDemonUnderlingCharges)
	{
		if (monsterUnderlingCharges.ContainsKey(p_monsterType))
		{
			p_monsterAndDemonUnderlingCharges = monsterUnderlingCharges[p_monsterType];
			return true;
		}
		p_monsterAndDemonUnderlingCharges = null;
		return false;
	}

	public bool HasMonsterUnderlingCharge(SUMMON_TYPE p_monsterType)
	{
		if (HasMonsterUnderlingEntry(p_monsterType))
		{
			return monsterUnderlingCharges[p_monsterType].currentCharges > 0;
		}
		return false;
	}

	public void AdjustMonsterUnderlingCharge(SUMMON_TYPE p_monsterType, int amount)
	{
		if (HasMonsterUnderlingEntry(p_monsterType))
		{
			MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges = monsterUnderlingCharges[p_monsterType];
			int currentCharges = monsterAndDemonUnderlingCharges.currentCharges;
			int currentCharges2 = monsterAndDemonUnderlingCharges.currentCharges;
			currentCharges2 += amount;
			if (currentCharges2 > monsterAndDemonUnderlingCharges.maxCharges)
			{
				currentCharges2 = monsterAndDemonUnderlingCharges.maxCharges;
			}
			else if (currentCharges2 < 0)
			{
				currentCharges2 = 0;
			}
			monsterAndDemonUnderlingCharges.currentCharges = currentCharges2;
			Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, monsterAndDemonUnderlingCharges);
			int num = monsterAndDemonUnderlingCharges.currentCharges - currentCharges;
			if (amount > 0 && num > 0)
			{
				SpawnMonsterUnderlings(monsterAndDemonUnderlingCharges, num);
			}
		}
		else
		{
			AddMonsterUnderlingEntry(p_monsterType, amount, amount, CharacterManager.Instance.GetSummonClassNameBySummonType(p_monsterType));
		}
		if (p_monsterType == SUMMON_TYPE.Fallen_Angel)
		{
			PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_FALLEN_ANGEL);
		}
	}

	public void AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE p_monsterType, int amount, bool adjustCurrentCharges = true)
	{
		if (HasMonsterUnderlingEntry(p_monsterType))
		{
			MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges = monsterUnderlingCharges[p_monsterType];
			int maxCharges = monsterAndDemonUnderlingCharges.maxCharges;
			maxCharges += amount;
			if (maxCharges < 0)
			{
				maxCharges = 0;
			}
			monsterAndDemonUnderlingCharges.maxCharges = maxCharges;
			if (adjustCurrentCharges)
			{
				AdjustMonsterUnderlingCharge(p_monsterType, amount);
				if (amount < 0)
				{
					monsterAndDemonUnderlingCharges.OnLoseMaxCharges();
				}
			}
			else
			{
				if (amount < 0)
				{
					monsterAndDemonUnderlingCharges.OnLoseMaxCharges();
				}
				Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, monsterAndDemonUnderlingCharges);
			}
		}
		else
		{
			AddMonsterUnderlingEntry(p_monsterType, adjustCurrentCharges ? amount : 0, amount, CharacterManager.Instance.GetSummonClassNameBySummonType(p_monsterType));
		}
	}

	public void DecreaseMonsterUnderlingCharge(SUMMON_TYPE p_monsterType)
	{
		if (HasMonsterUnderlingEntry(p_monsterType))
		{
			MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges = monsterUnderlingCharges[p_monsterType];
			monsterAndDemonUnderlingCharges.currentCharges--;
			if (monsterAndDemonUnderlingCharges.currentCharges < 0)
			{
				monsterAndDemonUnderlingCharges.currentCharges = 0;
			}
			Messenger.Broadcast(PlayerSignals.UPDATED_MONSTER_UNDERLING, monsterAndDemonUnderlingCharges);
			if (monsterAndDemonUnderlingCharges.ShouldStartMonsterReplenish())
			{
				monsterAndDemonUnderlingCharges.StartMonsterReplenish();
			}
			else
			{
				monsterAndDemonUnderlingCharges.CancelMonsterReplenish();
			}
		}
	}

	public MonsterAndDemonUnderlingCharges GetSummonUnderlingChargesBySummonType(SUMMON_TYPE p_type)
	{
		return monsterUnderlingCharges[p_type];
	}

	public bool IsMaximumNumberOfMonsterOfTypeSpawned(SUMMON_TYPE p_monsterType)
	{
		if (HasMonsterUnderlingEntry(p_monsterType))
		{
			MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges = monsterUnderlingCharges[p_monsterType];
			int num = 0;
			if (persistentDefendParty != null)
			{
				for (int i = 0; i < persistentDefendParty.members.Count; i++)
				{
					if (persistentDefendParty.members[i] is Summon summon && summon.summonType == p_monsterType)
					{
						num++;
					}
				}
			}
			return num >= monsterAndDemonUnderlingCharges.maxCharges;
		}
		return false;
	}

	public void TrySpawnMissingDemons(MINION_TYPE minionType)
	{
		if (!demonUnderlingCharges.ContainsKey(minionType))
		{
			return;
		}
		MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges = demonUnderlingCharges[minionType];
		int numberOfAliveMonstersInPlayerFaction = PlayerManager.Instance.player.GetNumberOfAliveMonstersInPlayerFaction(monsterAndDemonUnderlingCharges.minionType);
		int num = monsterAndDemonUnderlingCharges.maxCharges - numberOfAliveMonstersInPlayerFaction;
		if (num <= 0)
		{
			return;
		}
		MinionPlayerSkill minionPlayerSkillDataByMinionType = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(minionType);
		LocationStructure firstStructureOfType = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
		if (firstStructureOfType != null)
		{
			LocationGridTile centerTile = firstStructureOfType.GetCenterTile();
			for (int i = 0; i < num; i++)
			{
				minionPlayerSkillDataByMinionType.ActivateAbility(centerTile);
			}
		}
	}

	private void SpawnDemonUnderlings(MonsterAndDemonUnderlingCharges p_charges)
	{
		LocationStructure firstStructureOfType = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
		if (firstStructureOfType != null)
		{
			LocationGridTile centerTile = firstStructureOfType.GetCenterTile();
			int currentCharges = p_charges.currentCharges;
			MinionPlayerSkill minionPlayerSkillDataByMinionType = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_charges.minionType);
			for (int i = 0; i < currentCharges; i++)
			{
				minionPlayerSkillDataByMinionType.ActivateAbility(centerTile);
			}
		}
	}

	private void SpawnMonsterUnderlings(MonsterAndDemonUnderlingCharges p_charges, int p_amount)
	{
		LocationStructure firstStructureOfType = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
		if (firstStructureOfType == null)
		{
			return;
		}
		LocationGridTile centerTile = firstStructureOfType.GetCenterTile();
		SummonPlayerSkill summonPlayerSkillData = PlayerSkillManager.Instance.GetSummonPlayerSkillData(p_charges.monsterType);
		for (int i = 0; i < p_amount; i++)
		{
			if (IsMaximumNumberOfMonsterOfTypeSpawned(p_charges.monsterType))
			{
				break;
			}
			Character spawnedCharacter = null;
			summonPlayerSkillData.ActivateAbility(centerTile, ref spawnedCharacter);
			ConnectMonsterToDemonicStructure(spawnedCharacter as Summon);
		}
	}

	private void ConnectMonsterToDemonicStructure(Summon p_monster)
	{
		for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++)
		{
			if (PlayerManager.Instance.player.playerSettlement.allStructures[i] is DemonicStructure demonicStructure && demonicStructure.housedMonsterType == p_monster.summonType && !demonicStructure.HasMaximumConnectedMonsters())
			{
				demonicStructure.AddConnectedMonster(p_monster);
			}
		}
	}

	public void AddCharacterToPersistentDefendParty(Character p_character)
	{
		if (persistentDefendParty == null)
		{
			PlayerManager.Instance.player.playerFaction.partyQuestBoard.CreateDemonDefendPartyQuest(null, PlayerManager.Instance.player.playerSettlement).SetIsSuccessful(state: true);
			Party party = PartyManager.Instance.CreatePersistentDemonicDefendParty(p_character);
			party.SetDoNotDisband(state: true);
			party.TryAcceptQuest();
			party.AddMemberThatJoinedQuest(p_character);
			p_character.AddPlayerAction(PLAYER_SKILL_TYPE.UNSUMMON);
			return;
		}
		persistentDefendParty.AddMember(p_character);
		if (!persistentDefendParty.isActive)
		{
			PlayerManager.Instance.player.playerFaction.partyQuestBoard.CreateDemonDefendPartyQuest(null, PlayerManager.Instance.player.playerSettlement).SetIsSuccessful(state: true);
			persistentDefendParty.TryAcceptQuest();
		}
		persistentDefendParty.AddMemberThatJoinedQuest(p_character);
		p_character.AddPlayerAction(PLAYER_SKILL_TYPE.UNSUMMON);
	}

	public void OnCharacterRemovedFromPersistentDefenseParty(Character p_character)
	{
		p_character.RemovePlayerAction(PLAYER_SKILL_TYPE.UNSUMMON);
	}

	public void SetPersistentDefendParty(Party p_party)
	{
		persistentDefendParty = p_party;
	}

	public int GetDefendPartyMemberCount()
	{
		if (persistentDefendParty != null)
		{
			return persistentDefendParty.members.Count;
		}
		return 0;
	}

	public bool CanStillSpawnPlayerDefenders()
	{
		return PlayerManager.Instance.player.underlingsComponent.GetDefendPartyMemberCount() < PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForStructureType(STRUCTURE_TYPE.PRISM);
	}

	public bool IsInPersistentDefendParty(Character p_character)
	{
		if (persistentDefendParty != null)
		{
			return persistentDefendParty.IsMember(p_character);
		}
		return false;
	}

	public void LoadReferences(SaveDataPlayerUnderlingsComponent data)
	{
		foreach (MonsterAndDemonUnderlingCharges value in monsterUnderlingCharges.Values)
		{
			value.LoadMonsterReplenish();
		}
		if (string.IsNullOrEmpty(data.persistentDefendParty))
		{
			return;
		}
		persistentDefendParty = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentIDSafe(data.persistentDefendParty);
		if (persistentDefendParty != null && persistentDefendParty.members != null)
		{
			for (int i = 0; i < persistentDefendParty.members.Count; i++)
			{
				persistentDefendParty.members[i].AddPlayerAction(PLAYER_SKILL_TYPE.UNSUMMON, broadcastSignal: false);
			}
		}
	}
}
