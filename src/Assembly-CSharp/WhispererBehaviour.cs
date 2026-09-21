public class WhispererBehaviour : BaseMonsterBehaviour
{
	public WhispererBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (GameManager.Instance.GetCurrentTimeInWordsOfTick().IsDayTime())
		{
			if (character.HasHome() && !character.IsAtHome() && character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob))
			{
				return true;
			}
			return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE, out producedJob);
		}
		Area areaLocation = character.areaLocation;
		if (areaLocation != null)
		{
			Area area = areaLocation.neighbourComponent.GetRandomWildernessNeighbour();
			if (area == null)
			{
				area = character.currentRegion.GetRandomAreaThatIsInWilderness();
			}
			if (area != null)
			{
				return character.jobComponent.TriggerMoveToArea(JOB_TYPE.IDLE, out producedJob, area);
			}
		}
		return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE, out producedJob);
	}
}
