using UtilityScripts;

public class GhoulBehaviour : BaseMonsterBehaviour
{
	public GhoulBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (GameUtilities.RollChance(80))
		{
			return character.jobComponent.TriggerStand(out producedJob);
		}
		return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE, out producedJob);
	}
}
