using UtilityScripts;

public class GhostBehaviour : BaseMonsterBehaviour
{
	public GhostBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.gridTileLocation != null)
		{
			if (character.homeStructure == null)
			{
				if (character.HasTerritory() && character.territory.HasAliveVillagerResident())
				{
					character.ClearTerritory();
				}
				if (!character.HasTerritory())
				{
					Area area = character.areaLocation?.neighbourComponent.GetNearestPlainAreaWithNoResident();
					if (area != null)
					{
						character.SetTerritory(area);
					}
				}
			}
			else if (character.HasTerritory())
			{
				character.ClearTerritory();
			}
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
			if ((currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT) && TryDoRevenge(character, ref log, out producedJob))
			{
				return true;
			}
			if (character.homeStructure != null)
			{
				if (!character.isAtHomeStructure)
				{
					return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
				}
				return character.jobComponent.TriggerRoamAroundTile(out producedJob);
			}
			if (character.HasTerritory() && !character.IsInTerritory())
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
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if ((currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT) && GameUtilities.RollChance(5, ref p_log) && TryDoRevenge(p_character, ref p_log, out p_producedJob))
		{
			return true;
		}
		return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
	}

	private bool TryDoRevenge(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (p_character is Ghost { betrayedBy: not null } ghost && !ghost.betrayedBy.isDead && ghost.betrayedBy.currentRegion == ghost.currentRegion && ghost.jobComponent.CreateSlayTargetJob(ghost.betrayedBy, out p_producedJob))
		{
			return true;
		}
		p_producedJob = null;
		return false;
	}
}
