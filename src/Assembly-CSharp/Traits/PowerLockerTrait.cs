using System;

namespace Traits;

public abstract class PowerLockerTrait : Trait
{
	public PLAYER_SKILL_TYPE lockedSkill { get; private set; }

	public override Type serializedData => typeof(SaveDataPowerLockerTrait);

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character { isDead: false } character)
		{
			CharacterManager.Instance.AddPowerLocker(character);
		}
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		if (p_saveDataTrait is SaveDataPowerLockerTrait saveDataPowerLockerTrait)
		{
			lockedSkill = saveDataPowerLockerTrait.lockedSkill;
		}
	}

	public void LockRandomPlayerSkill(Character p_character)
	{
		if (PlayerManager.Instance.player != null)
		{
			PLAYER_SKILL_TYPE randomSkillTypeToLock = PlayerManager.Instance.player.playerSkillComponent.GetRandomSkillTypeToLock();
			lockedSkill = randomSkillTypeToLock;
			if (lockedSkill != PLAYER_SKILL_TYPE.NONE)
			{
				PlayerManager.Instance.player.playerSkillComponent.LockSkill(lockedSkill, p_character);
			}
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character p_character)
		{
			LockRandomPlayerSkill(p_character);
			CharacterManager.Instance.AddPowerLocker(p_character);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character p_character)
		{
			if (lockedSkill != PLAYER_SKILL_TYPE.NONE)
			{
				PlayerManager.Instance.player.playerSkillComponent.UnlockSkill(lockedSkill);
			}
			lockedSkill = PLAYER_SKILL_TYPE.NONE;
			CharacterManager.Instance.RemovePowerLocker(p_character);
		}
	}

	public override bool OnDeath(Character character)
	{
		if (lockedSkill != PLAYER_SKILL_TYPE.NONE)
		{
			PlayerManager.Instance.player.playerSkillComponent.UnlockSkill(lockedSkill);
		}
		lockedSkill = PLAYER_SKILL_TYPE.NONE;
		CharacterManager.Instance.RemovePowerLocker(character);
		return base.OnDeath(character);
	}

	protected override string GetDescriptionInUI()
	{
		string text = base.GetDescriptionInUI();
		if (lockedSkill != PLAYER_SKILL_TYPE.NONE)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(lockedSkill);
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Locked_Skills") + ": " + skillData.localizedName;
		}
		return text;
	}
}
