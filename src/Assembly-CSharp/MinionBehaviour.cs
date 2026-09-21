public class MinionBehaviour : CharacterBehaviour
{
	public MinionBehaviour()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		character.jobComponent.PlanIdleStrollOutside(out producedJob);
		return true;
	}
}
