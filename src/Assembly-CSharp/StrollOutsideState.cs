public class StrollOutsideState : CharacterState
{
	public StrollOutsideState(CharacterStateComponent characterComp)
		: base(characterComp)
	{
		base.stateName = "Stroll Outside State";
		base.characterState = CHARACTER_STATE.STROLL_OUTSIDE;
		base.duration = 20;
	}

	protected override void DoMovementBehavior()
	{
		base.DoMovementBehavior();
		StartStrollMovement();
	}

	public void StartStrollMovement()
	{
		base.stateComponent.owner.marker.DoStrollMovement();
	}
}
