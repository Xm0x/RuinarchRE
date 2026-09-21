public class DazedBehaviour : CharacterBehaviour
{
	public DazedBehaviour()
	{
		base.priority = 10;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeDazed();
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.behaviourComponent.OnNoLongerDazed();
	}

	public override void OnLoadBehaviourToCharacter(Character character)
	{
		base.OnLoadBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeDazed();
	}
}
