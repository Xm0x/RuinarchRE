using System;
using Inner_Maps.Location_Structures;
using UnityEngine;

[Serializable]
public class MonsterAndDemonUnderlingCharges
{
	public SUMMON_TYPE monsterType;

	public MINION_TYPE minionType;

	public int currentCharges;

	public int maxCharges;

	public string characterClassName;

	public bool isDemon;

	public bool isReplenishing;

	public GameDate replenishDate;

	public int currentCooldownTick;

	public bool hasMaxCharge => maxCharges > 0;

	public int cooldown => PlayerManager.Instance.player.underlingsComponent.cooldown;

	public MonsterAndDemonUnderlingCharges(SUMMON_TYPE p_monsterType, int p_currentCharges, int p_maxCharges, string p_characterClassName)
	{
		monsterType = p_monsterType;
		currentCharges = p_currentCharges;
		maxCharges = p_maxCharges;
		characterClassName = p_characterClassName;
	}

	public MonsterAndDemonUnderlingCharges(MINION_TYPE p_minionType, int p_currentCharges, int p_maxCharges, string p_characterClassName)
	{
		minionType = p_minionType;
		currentCharges = p_currentCharges;
		maxCharges = p_maxCharges;
		characterClassName = p_characterClassName;
		isDemon = true;
	}

	public void StartMonsterReplenish()
	{
		if (!isReplenishing)
		{
			isReplenishing = true;
			currentCooldownTick = 0;
			replenishDate = GameManager.Instance.Today().AddTicks(cooldown);
			if (cooldown > 0)
			{
				Messenger.AddListener(Signals.TICK_STARTED, PerTickReplenish);
			}
			else
			{
				PerTickReplenish();
			}
			Messenger.Broadcast(PlayerSkillSignals.START_MONSTER_UNDERLING_COOLDOWN, this);
		}
	}

	public void OnLoseMaxCharges()
	{
		currentCharges = Mathf.Clamp(currentCharges, 0, maxCharges);
		if (!hasMaxCharge)
		{
			CancelMonsterReplenish();
		}
	}

	private void PerTickReplenish()
	{
		currentCooldownTick++;
		Messenger.Broadcast(PlayerSkillSignals.PER_TICK_MONSTER_UNDERLING_COOLDOWN, this);
		if (currentCooldownTick >= cooldown)
		{
			Messenger.Broadcast(PlayerSkillSignals.ON_FINISH_UNDERLING_COOLDOWN, this);
			DoneMonsterReplenish();
		}
	}

	private void DoneMonsterReplenish()
	{
		if (isReplenishing)
		{
			currentCooldownTick = 0;
			Messenger.RemoveListener(Signals.TICK_STARTED, PerTickReplenish);
			isReplenishing = false;
			Messenger.Broadcast(PlayerSkillSignals.STOP_MONSTER_UNDERLING_COOLDOWN, this);
			ReplenishCharges();
		}
	}

	public void CancelMonsterReplenish()
	{
		if (isReplenishing)
		{
			currentCooldownTick = 0;
			Messenger.RemoveListener(Signals.TICK_STARTED, PerTickReplenish);
			isReplenishing = false;
			Messenger.Broadcast(PlayerSkillSignals.STOP_MONSTER_UNDERLING_COOLDOWN, this);
		}
	}

	private void ReplenishCharges()
	{
		if (!PlayerManager.Instance.player.underlingsComponent.HasMonsterUnderlingEntry(monsterType))
		{
			return;
		}
		int chargesToReplenishFor = GetChargesToReplenishFor(monsterType);
		if (chargesToReplenishFor > 0)
		{
			PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(monsterType, chargesToReplenishFor);
			if (ShouldStartMonsterReplenish())
			{
				StartMonsterReplenish();
			}
		}
	}

	public bool ShouldStartMonsterReplenish()
	{
		if (currentCharges < maxCharges && hasMaxCharge && !PlayerManager.Instance.player.underlingsComponent.IsMaximumNumberOfMonsterOfTypeSpawned(monsterType))
		{
			return true;
		}
		return false;
	}

	private int GetChargesToReplenishFor(SUMMON_TYPE p_monsterType)
	{
		int num = 0;
		for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++)
		{
			if (PlayerManager.Instance.player.playerSettlement.allStructures[i] is DemonicStructure demonicStructure && demonicStructure.housedMonsterType == p_monsterType)
			{
				num++;
			}
		}
		return num;
	}

	public bool CanBeCasted()
	{
		return currentCharges > 0;
	}

	public bool CanBeCastedAsPartOfParty()
	{
		if (isDemon)
		{
			return PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(minionType).CanPerformAbility();
		}
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(characterClassName);
		if (characterClass.combatBehaviourType != CHARACTER_COMBAT_BEHAVIOUR.Tower && PlayerManager.Instance.player.currenciesComponent.mana >= characterClass.GetSummonCost())
		{
			return currentCharges > 0;
		}
		return false;
	}

	public string GetReasonsWhyCannotBeCastedAsPartOfParty()
	{
		string text = string.Empty;
		if (isDemon)
		{
			SkillData minionPlayerSkillDataByMinionType = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(minionType);
			if (minionPlayerSkillDataByMinionType.hasManaCost && PlayerManager.Instance.player.currenciesComponent.mana < minionPlayerSkillDataByMinionType.manaCost)
			{
				text = text + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Mana") + "\n";
			}
			if (minionPlayerSkillDataByMinionType.hasCharges && minionPlayerSkillDataByMinionType.charges <= 0 && !minionPlayerSkillDataByMinionType.hasBonusCharges)
			{
				text += LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Charges");
			}
		}
		else
		{
			CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(characterClassName);
			if (PlayerManager.Instance.player.currenciesComponent.mana < characterClass.GetSummonCost())
			{
				text = text + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Mana") + ".\n";
			}
			if (currentCharges <= 0)
			{
				text = text + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Charges") + ".\n";
			}
			if (characterClass.combatBehaviourType == CHARACTER_COMBAT_BEHAVIOUR.Tower)
			{
				text += LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Tower_Cannot_Summon");
			}
		}
		return text;
	}

	public void LoadMonsterReplenish()
	{
		if (isReplenishing)
		{
			Messenger.AddListener(Signals.TICK_STARTED, PerTickReplenish);
		}
	}
}
