using UtilityScripts;

public class CombatSpecialSkillWrapper
{
	public CombatSpecialSkill specialSkill { get; private set; }

	public int currentCooldown { get; private set; }

	public bool isInCooldown
	{
		get
		{
			if (specialSkill != null)
			{
				return currentCooldown < specialSkill.cooldownInTicks;
			}
			return false;
		}
	}

	public CombatSpecialSkillWrapper()
	{
	}

	public CombatSpecialSkillWrapper(SaveDataCombatSpecialSkillWrapper data)
	{
		specialSkill = CombatManager.Instance.GetCombatSpecialSkill(data.specialSkill);
		currentCooldown = data.currentCooldown;
	}

	private void OnTickStarted()
	{
		PerTickCooldown();
	}

	private void SetSpecialSkill(CombatSpecialSkill p_skill)
	{
		if (specialSkill != p_skill)
		{
			specialSkill = p_skill;
			if (specialSkill != null)
			{
				ResetCooldown();
			}
		}
	}

	public void SetSpecialSkill(COMBAT_SPECIAL_SKILL p_skillType)
	{
		CombatSpecialSkill combatSpecialSkill = CombatManager.Instance.GetCombatSpecialSkill(p_skillType);
		SetSpecialSkill(combatSpecialSkill);
	}

	public void SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL p_skillType)
	{
		int num = ((specialSkill != null) ? specialSkill.tier : 0);
		CombatSpecialSkill combatSpecialSkill = CombatManager.Instance.GetCombatSpecialSkill(p_skillType);
		if (num < combatSpecialSkill.tier || (num == combatSpecialSkill.tier && GameUtilities.RollChance(50)))
		{
			SetSpecialSkill(combatSpecialSkill);
		}
	}

	public bool TryActivateSpecialSkill(Character p_character)
	{
		if (!isInCooldown && specialSkill != null)
		{
			bool flag = false;
			Character rider = p_character.mountComponent.GetRider();
			if ((rider != null) ? specialSkill.TryActivateSkill(p_character, rider) : specialSkill.TryActivateSkill(p_character))
			{
				StartCooldown();
			}
		}
		return false;
	}

	public bool HasSpecialSkill()
	{
		return specialSkill != null;
	}

	public void StartCooldown()
	{
		currentCooldown = 0;
		if (specialSkill.cooldownInTicks > 0)
		{
			Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
		}
		else
		{
			currentCooldown = specialSkill.cooldownInTicks;
		}
	}

	private void PerTickCooldown()
	{
		currentCooldown++;
		if (!isInCooldown)
		{
			if (specialSkill != null)
			{
				currentCooldown = specialSkill.cooldownInTicks;
			}
			Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
		}
	}

	private void ResetCooldown()
	{
		currentCooldown = specialSkill.cooldownInTicks;
	}

	public void LoadReferences()
	{
		if (isInCooldown)
		{
			Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
		}
	}
}
