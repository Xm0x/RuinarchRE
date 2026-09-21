namespace Plague.Death_Effect;

public class ChaosGenerator : PlagueDeathEffect
{
	public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Chaos_Generator;

	protected override void ActivateEffect(Character p_character)
	{
		switch (_level)
		{
		case 1:
			CreateChaosOrbs(1, p_character);
			break;
		case 2:
			CreateChaosOrbs(2, p_character);
			break;
		case 3:
			CreateChaosOrbs(3, p_character);
			break;
		}
	}

	protected override int GetNextLevelUpgradeCost()
	{
		return _level switch
		{
			1 => 50, 
			2 => 75, 
			_ => -1, 
		};
	}

	public override string GetCurrentEffectDescription()
	{
		return _level switch
		{
			1 => "1 " + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Orbs"), 
			2 => "2 " + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Orbs"), 
			3 => "3 " + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Orbs"), 
			_ => string.Empty, 
		};
	}

	public override void OnDeath(Character p_character)
	{
		ActivateEffectOn(p_character);
	}

	private void CreateChaosOrbs(int amount, Character p_character)
	{
		if ((bool)p_character.marker && p_character.currentRegion != null)
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.marker.transform.position, amount, p_character.currentRegion.innerMap);
		}
	}
}
