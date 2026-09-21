public class HarassBehaviour : CharacterBehaviour
{
	public HarassBehaviour()
	{
		base.priority = 10;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		return false;
	}
}
