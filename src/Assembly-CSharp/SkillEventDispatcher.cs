using System;

public class SkillEventDispatcher
{
	public interface ISkillLevelUpListener
	{
		void OnSkillLeveledUp(SkillData p_skillData, PlayerSkillData p_playerSkillData);
	}

	private Action<SkillData, PlayerSkillData> _onSkillUpgraded;

	public void SubscribeToLevelUp(ISkillLevelUpListener p_listener)
	{
		_onSkillUpgraded = (Action<SkillData, PlayerSkillData>)Delegate.Combine(_onSkillUpgraded, new Action<SkillData, PlayerSkillData>(p_listener.OnSkillLeveledUp));
	}

	public void UnsubscribeToLevelUp(ISkillLevelUpListener p_listener)
	{
		_onSkillUpgraded = (Action<SkillData, PlayerSkillData>)Delegate.Remove(_onSkillUpgraded, new Action<SkillData, PlayerSkillData>(p_listener.OnSkillLeveledUp));
	}

	public void ExecuteLevelUpEvent(SkillData p_skillData, PlayerSkillData p_playerSkillData)
	{
		_onSkillUpgraded?.Invoke(p_skillData, p_playerSkillData);
	}

	public void CleanUp()
	{
		_onSkillUpgraded = null;
	}
}
