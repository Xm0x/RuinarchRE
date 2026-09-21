public class DefaultMinion : CharacterBehaviour
{
	public DefaultMinion()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		character.jobComponent.TriggerRoamAroundTile(out producedJob);
		return true;
	}
}
