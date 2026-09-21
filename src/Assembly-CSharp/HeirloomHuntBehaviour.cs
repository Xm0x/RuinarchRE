public class HeirloomHuntBehaviour : CharacterBehaviour
{
	public HeirloomHuntBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return true;
	}
}
