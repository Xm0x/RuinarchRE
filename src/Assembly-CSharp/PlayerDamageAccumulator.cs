using Inner_Maps;

public class PlayerDamageAccumulator
{
	public int accumulatedDamage { get; private set; }

	public bool activatedSpellDamageChaosOrbPassiveSkill { get; private set; }

	public PlayerDamageAccumulator()
	{
	}

	public PlayerDamageAccumulator(SaveDataPlayerDamageAccumulator data)
	{
		accumulatedDamage = data.accumulatedDamage;
		activatedSpellDamageChaosOrbPassiveSkill = data.activatedSpellDamageChaosOrbPassiveSkill;
	}

	public void SetActivatedSpellDamageChaosOrbPassiveSkill(bool p_state)
	{
		activatedSpellDamageChaosOrbPassiveSkill = p_state;
	}

	private void AccumulateDamage(int p_amount)
	{
		if (p_amount < 0)
		{
			p_amount *= -1;
		}
		accumulatedDamage += p_amount;
		PlayerUI.Instance.UpdateAccumulatedDamageText(accumulatedDamage);
	}

	public void AccumulateDamage(int p_amount, LocationGridTile p_expelChaosOrbsOn, Character p_character)
	{
		if (!activatedSpellDamageChaosOrbPassiveSkill)
		{
			return;
		}
		_ = string.Empty;
		AccumulateDamage(p_amount);
		int chaosOrbExpulsionThreshold = PlayerManager.Instance.player.playerSkillComponent.chaosOrbExpulsionThreshold;
		if (p_expelChaosOrbsOn != null && accumulatedDamage >= chaosOrbExpulsionThreshold)
		{
			int num = accumulatedDamage / chaosOrbExpulsionThreshold;
			int num2 = num * chaosOrbExpulsionThreshold;
			accumulatedDamage -= num2;
			if (num > 0)
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_expelChaosOrbsOn.centeredWorldLocation, num, p_expelChaosOrbsOn.parentMap);
			}
		}
	}
}
