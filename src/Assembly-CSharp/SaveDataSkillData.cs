using System;

[Serializable]
public class SaveDataSkillData : SaveData<SkillData>
{
	public PLAYER_SKILL_TYPE type;

	public int maxCharges;

	public int charges;

	public int bonusCharges;

	public int manaCost;

	public int baseSpiritEnergyCost;

	public int cooldown;

	public int unlockCost;

	public int currentLevel;

	public int currentCooldownTick;

	public int remainingChaosOrbs;

	public float basePierce;

	public bool isUnlockedBaseOnRequirements;

	public bool isInUse;

	public bool isTemporarilyInUse;

	public bool isUsable;

	public string characterThatLockedSkillID;

	public override void Save(SkillData data)
	{
		type = data.type;
		maxCharges = data.baseMaxCharges;
		charges = data.charges;
		bonusCharges = data.bonusCharges;
		manaCost = data.baseManaCost;
		baseSpiritEnergyCost = data.baseSpiritEnergyCost;
		cooldown = data.baseCooldown;
		currentCooldownTick = data.currentCooldownTick;
		currentLevel = data.currentLevel;
		basePierce = data.basePierce;
		unlockCost = data.unlockCost;
		isUnlockedBaseOnRequirements = data.isUnlockedBaseOnRequirements;
		isInUse = data.isInUse;
		isTemporarilyInUse = data.isTemporarilyInUse;
		isUsable = data.isUsable;
		characterThatLockedSkillID = data.characterThatLockedSkillID;
		remainingChaosOrbs = data.remainingChaosOrbs;
	}

	public override SkillData Load()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(type);
		skillData.SetCharges(charges);
		skillData.SetBonusCharges(bonusCharges);
		skillData.SetCooldown(cooldown);
		skillData.SetManaCost(manaCost);
		skillData.SetBaseSpiritEnergyCost(baseSpiritEnergyCost);
		skillData.SetPierce(basePierce);
		skillData.SetUnlockCost(unlockCost);
		skillData.SetCurrentCooldownTick(currentCooldownTick);
		skillData.SetMaxCharges(maxCharges);
		skillData.SetCurrentLevel(currentLevel);
		skillData.SetIsUnlockBaseOnRequirements(isUnlockedBaseOnRequirements);
		skillData.SetIsInUse(isInUse);
		skillData.SetIsTemporarilyInUse(isTemporarilyInUse);
		skillData.SetIsUsable(isUsable);
		skillData.SetCharacterThatLockedSkill(characterThatLockedSkillID);
		skillData.SetRemainingChaosOrbs(remainingChaosOrbs);
		return skillData;
	}
}
