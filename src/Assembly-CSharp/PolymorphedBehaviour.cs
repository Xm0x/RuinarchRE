public class PolymorphedBehaviour : CharacterBehaviour
{
	public PolymorphedBehaviour()
	{
		base.priority = 41;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
	}
}
