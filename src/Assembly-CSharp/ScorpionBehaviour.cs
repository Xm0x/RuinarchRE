public class ScorpionBehaviour : BaseMonsterBehaviour
{
	public ScorpionBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) >= 18 || GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) < 6)
		{
			if ((character as Scorpion).heldCharacter == null)
			{
				character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
				character.reactionComponent.SetIsHidden(state: false);
				return character.jobComponent.TriggerRoamAroundTerritory(out producedJob, checkIfPathPossibleWithoutDigging: true);
			}
			return false;
		}
		character.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
		character.reactionComponent.SetIsHidden(state: true);
		return character.jobComponent.PlanIdleLongStandStill(out producedJob);
	}

	protected override bool MonsterUnderlingBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		p_producedJob = null;
		return false;
	}
}
