using System;

[Serializable]
public class SaveDataCombatSpecialSkillWrapper : SaveData<CombatSpecialSkillWrapper>
{
	public COMBAT_SPECIAL_SKILL specialSkill { get; private set; }

	public int currentCooldown { get; private set; }

	public override void Save(CombatSpecialSkillWrapper data)
	{
		base.Save(data);
		specialSkill = COMBAT_SPECIAL_SKILL.None;
		if (data.specialSkill != null)
		{
			specialSkill = data.specialSkill.specialSkillType;
		}
		currentCooldown = data.currentCooldown;
	}

	public override CombatSpecialSkillWrapper Load()
	{
		return new CombatSpecialSkillWrapper(this);
	}
}
