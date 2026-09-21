using UnityEngine;
using UtilityScripts;

public class RevenantBehaviour : BaseMonsterBehaviour
{
	public RevenantBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.gridTileLocation != null && (character.isAtHomeStructure || character.IsInHomeSettlement() || character.IsInTerritory()))
		{
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
			if ((currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT) && Random.Range(0, 100) < 10 && character is Revenant revenant)
			{
				revenant.TrySpawnGhost(ref log);
			}
			if (character.HasHome() && !character.IsAtHome())
			{
				return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
			}
			return character.jobComponent.TriggerRoamAroundTile(out producedJob);
		}
		return false;
	}

	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob))
		{
			return true;
		}
		if (GameUtilities.RollChance(10) && p_character is Revenant revenant && revenant.TrySpawnGhost(ref p_log))
		{
			return false;
		}
		return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
	}
}
