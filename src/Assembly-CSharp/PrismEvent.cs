public class PrismEvent
{
	public PrismEventData data { get; protected set; }

	public bool isActivated { get; protected set; }

	public PrismEventRequirement[] requirements { get; protected set; }

	public PrismEvent(PrismEventData p_data)
	{
		data = p_data;
	}

	protected bool AreRequirementsMetBase()
	{
		if (requirements == null || requirements.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < requirements.Length; i++)
		{
			if (!requirements[i].IsRequirementSatisfied())
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void TriggerEventBase()
	{
	}

	public void SetIsActivated(bool p_state)
	{
		isActivated = p_state;
	}

	public bool AreRequirementsMet()
	{
		if (AreRequirementsMetBase() && PlayerManager.Instance.player.currenciesComponent.mana >= GetManaCost())
		{
			return !isActivated;
		}
		return false;
	}

	public void TriggerEvent()
	{
		TriggerEventBase();
		SetIsActivated(p_state: true);
		PlayerManager.Instance.player.currenciesComponent.AdjustMana(-GetManaCost());
	}

	public int GetManaCost()
	{
		int manaCost = data.manaCost;
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None)
		{
			return 0;
		}
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.Half)
		{
			return (int)((float)manaCost * 0.5f);
		}
		return manaCost;
	}
}
