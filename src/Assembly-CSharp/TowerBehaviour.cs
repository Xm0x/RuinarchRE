public class TowerBehaviour : CharacterBehaviour
{
	public TowerBehaviour()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		return true;
	}
}
