using System.Collections.Generic;

public class GroupedSkillSlotItems
{
	public List<SkillSlotItem> fixedSkillSlotItems { get; private set; }

	public List<SkillSlotItem> extraSkillSlotItems { get; private set; }

	public GroupedSkillSlotItems()
	{
		fixedSkillSlotItems = new List<SkillSlotItem>();
		extraSkillSlotItems = new List<SkillSlotItem>();
	}

	public void AddFixedSkillSlotItem(SkillSlotItem skillSlotItem)
	{
		fixedSkillSlotItems.Add(skillSlotItem);
	}

	public bool RemoveFixedSkillSlotItem(SkillSlotItem skillSlotItem)
	{
		return fixedSkillSlotItems.Remove(skillSlotItem);
	}

	public void ClearFixedSkillSlotItem()
	{
		fixedSkillSlotItems.Clear();
	}

	public void AddExtraSkillSlotItem(SkillSlotItem skillSlotItem)
	{
		extraSkillSlotItems.Add(skillSlotItem);
	}

	public bool RemoveExtraSkillSlotItem(SkillSlotItem skillSlotItem)
	{
		return extraSkillSlotItems.Remove(skillSlotItem);
	}

	public void ClearExtraSkillSlotItem()
	{
		extraSkillSlotItems.Clear();
	}

	public bool HasExtraSkill(PLAYER_SKILL_TYPE skillType)
	{
		for (int i = 0; i < extraSkillSlotItems.Count; i++)
		{
			if (extraSkillSlotItems[i].skillData != null && extraSkillSlotItems[i].skillData.skill == skillType)
			{
				return true;
			}
		}
		return false;
	}
}
