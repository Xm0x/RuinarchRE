using Inner_Maps.Location_Structures;

public class SkillProgressionManager
{
	public int CheckRequirementsAndGetUnlockCost(PlayerSkillComponent p_skills, FakeCurrenciesComponent pFakeCurrencies, PLAYER_SKILL_TYPE p_type)
	{
		return CheckRequirement(p_skills, pFakeCurrencies.Mana, p_type);
	}

	public int CheckRequirementsAndGetUnlockCost(PlayerSkillComponent p_skills, int p_mana, PLAYER_SKILL_TYPE p_type)
	{
		return CheckRequirement(p_skills, p_mana, p_type);
	}

	private int CheckRequirement(PlayerSkillComponent p_availablePlayerSkills, int p_mana, PLAYER_SKILL_TYPE p_type)
	{
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		for (int i = 0; i < scriptableObjPlayerSkillData.requirementData.requiredSkills.Count; i++)
		{
			if (!p_availablePlayerSkills.CheckIfSkillIsAvailable(scriptableObjPlayerSkillData.requirementData.requiredSkills[i]))
			{
				return -1;
			}
		}
		if (scriptableObjPlayerSkillData.tier <= 0)
		{
			return -1;
		}
		if (scriptableObjPlayerSkillData.requirementData.requiredArchetypes.Count > 0 && !scriptableObjPlayerSkillData.requirementData.requiredArchetypes.Contains(PlayerSkillManager.Instance.selectedArchetype))
		{
			return -1;
		}
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		int num = ((!thePortal.IsMaxLevel()) ? thePortal.level : 7);
		if (scriptableObjPlayerSkillData.requirementData.portalLevel <= num)
		{
			return scriptableObjPlayerSkillData.GetUnlockCost();
		}
		return -1;
	}
}
