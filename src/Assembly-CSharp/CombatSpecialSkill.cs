using System.Collections.Generic;
using UtilityScripts;

public class CombatSpecialSkill
{
	public COMBAT_SPECIAL_SKILL specialSkillType { get; }

	public COMBAT_SPECIAL_SKILL_TARGET targetType { get; }

	public COMBAT_SPECIAL_SKILL_CATEGORY category { get; }

	public int cooldownInTicks { get; }

	public int tier { get; }

	public string name { get; }

	public CombatSpecialSkill(COMBAT_SPECIAL_SKILL p_specialSkillType, COMBAT_SPECIAL_SKILL_TARGET p_targetType, COMBAT_SPECIAL_SKILL_CATEGORY p_category, int p_cooldownInTicks, int p_tier)
	{
		name = Utilities.NotNormalizedConversionEnumToString(p_specialSkillType.ToString());
		specialSkillType = p_specialSkillType;
		targetType = p_targetType;
		category = p_category;
		cooldownInTicks = p_cooldownInTicks;
		tier = p_tier;
	}

	public virtual void SetSpecialSkill(Character p_character)
	{
	}

	public virtual void UnsetSpecialSkill(Character p_character)
	{
	}

	public virtual bool TryActivateSkill(Character p_character)
	{
		return false;
	}

	public virtual bool TryActivateSkill(Character p_character, Character p_rider)
	{
		return false;
	}

	protected virtual Character GetValidTargetFor(Character p_character)
	{
		return null;
	}

	protected virtual void PopulateValidTargetsFor(Character p_character, List<Character> p_validTargets)
	{
	}

	public virtual bool CanBeLearnedByCharacter(Character p_character)
	{
		return true;
	}
}
