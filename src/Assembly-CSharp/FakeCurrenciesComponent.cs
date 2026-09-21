using System;

[Serializable]
public class FakeCurrenciesComponent
{
	private int m_mana;

	private int m_chaoticEnergy;

	private int m_spirits;

	public int Mana => m_mana;

	public int ChaoticEnergy => m_chaoticEnergy;

	public int Spirits => m_spirits;

	public FakeCurrenciesComponent()
	{
		m_mana = 35;
		m_chaoticEnergy = 35;
		m_spirits = 35;
	}

	public FakeCurrenciesComponent(SaveDataFakeCurrenciesComponent p_component)
	{
		m_mana = p_component.mana;
		m_chaoticEnergy = p_component.chaoticEnergy;
		m_spirits = p_component.spirits;
	}

	public void AdjustPlaguePoints(int amount)
	{
		if (!(WorldSettings.Instance != null) || WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount != SKILL_COST_AMOUNT.None)
		{
			m_mana += amount;
			Messenger.Broadcast(PlayerSignals.UPDATED_CHAOTIC_ENERGY, m_mana);
		}
	}

	public void GainPlaguePointFromCharacter(int amount, Character p_character)
	{
		AdjustPlaguePoints(amount);
	}

	public bool CanGainPlaguePoints()
	{
		if (PlayerManager.Instance.player.playerSettlement != null)
		{
			return PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB);
		}
		return false;
	}
}
