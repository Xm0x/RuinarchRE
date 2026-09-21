using UnityEngine;
using UnityEngine.Animations;

public class IdleAnimationStateMachine : StateMachineBehaviour
{
	private CharacterMarker _marker;

	public void SetCharacterMarker(CharacterMarker p_marker)
	{
		_marker = p_marker;
	}

	public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
	{
		_marker?.SetCurrentAnimationName("Idle");
	}
}
