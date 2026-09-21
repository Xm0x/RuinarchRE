using Inner_Maps.Location_Structures;

public class PartyDamageAccumulator
{
	public int accumulatedDamageFromDemonRaid { get; private set; }

	public int accumulatedDamageFromMonsterSpawnerRaid { get; private set; }

	public void Initialize(SaveDataPartyDamageAccumulator partyDamageAccumulator)
	{
		accumulatedDamageFromDemonRaid = partyDamageAccumulator.accumulatedDamageFromDemonRaid;
		accumulatedDamageFromMonsterSpawnerRaid = partyDamageAccumulator.accumulatedDamageFromMonsterSpawnerRaid;
	}

	private void AccumulateDamageFromDemonRaid(int p_amount)
	{
		if (p_amount < 0)
		{
			p_amount *= -1;
		}
		accumulatedDamageFromDemonRaid += p_amount;
	}

	public void AccumulateDamageFromDemonRaid(int p_amount, Character p_character)
	{
		_ = string.Empty;
		AccumulateDamageFromDemonRaid(p_amount);
		int chaosOrbExpulsionThresholdFromRaid = PlayerManager.Instance.player.playerSkillComponent.chaosOrbExpulsionThresholdFromRaid;
		if (p_character != null && accumulatedDamageFromDemonRaid >= chaosOrbExpulsionThresholdFromRaid)
		{
			int p_amount2 = accumulatedDamageFromDemonRaid / chaosOrbExpulsionThresholdFromRaid;
			int num = p_amount2 * chaosOrbExpulsionThresholdFromRaid;
			accumulatedDamageFromDemonRaid -= num;
			if (p_amount2 > 0 && PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MARAUD).TryDecreaseRemainingChaosOrbs(ref p_amount2))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, p_amount2, p_character.gridTileLocation.parentMap);
			}
		}
	}

	private void AccumulateDamageFromMonsterSpawnerRaid(int p_amount)
	{
		if (p_amount < 0)
		{
			p_amount *= -1;
		}
		accumulatedDamageFromMonsterSpawnerRaid += p_amount;
	}

	public void AccumulateDamageMonsterSpawnerRaid(int p_amount, Character p_character)
	{
		_ = string.Empty;
		AccumulateDamageFromMonsterSpawnerRaid(p_amount);
		int chaosOrbExpulsionThresholdFromRaid = PlayerManager.Instance.player.playerSkillComponent.chaosOrbExpulsionThresholdFromRaid;
		if (p_character != null && accumulatedDamageFromMonsterSpawnerRaid >= chaosOrbExpulsionThresholdFromRaid)
		{
			int p_amount2 = accumulatedDamageFromMonsterSpawnerRaid / chaosOrbExpulsionThresholdFromRaid;
			int num = p_amount2 * chaosOrbExpulsionThresholdFromRaid;
			accumulatedDamageFromMonsterSpawnerRaid -= num;
			if (p_amount2 > 0 && PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER).TryDecreaseRemainingChaosOrbs(ref p_amount2))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, p_amount2, p_character.gridTileLocation.parentMap);
			}
		}
	}

	public void Reset()
	{
		accumulatedDamageFromDemonRaid = 0;
		accumulatedDamageFromMonsterSpawnerRaid = 0;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
