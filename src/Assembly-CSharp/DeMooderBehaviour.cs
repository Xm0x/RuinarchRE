public class DeMooderBehaviour : CharacterBehaviour
{
	public DeMooderBehaviour()
	{
		base.priority = 30;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		return character.jobComponent.TriggerDecreaseMood(out producedJob);
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.traitContainer.AddTrait(character, "Stealthy");
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.traitContainer.RemoveTrait(character, "Stealthy");
	}
}
