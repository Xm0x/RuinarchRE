public class NatureSpiritBehaviour : BaseMonsterBehaviour
{
	public NatureSpiritBehaviour()
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
}
