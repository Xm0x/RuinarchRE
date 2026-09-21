public class FallenAngelBehaviour : BaseMonsterBehaviour
{
	public FallenAngelBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.IsAtHome())
		{
			return character.jobComponent.TriggerRoamAroundTile(out producedJob);
		}
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}

	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (p_character.IsAtHome())
		{
			return p_character.jobComponent.TriggerRoamAroundTile(out p_producedJob);
		}
		return p_character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out p_producedJob);
	}
}
