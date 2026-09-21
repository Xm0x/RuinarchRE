public class ZombieBehaviour : CharacterBehaviour
{
	public ZombieBehaviour()
	{
		base.priority = 900;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		return character.jobComponent.PlanZombieStrollOutside(out producedJob);
	}
}
