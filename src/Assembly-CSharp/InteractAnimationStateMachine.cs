using UnityEngine;

public class InteractAnimationStateMachine : StateMachineBehaviour
{
	private CharacterMarker _marker;

	public void SetCharacterMarker(CharacterMarker p_marker)
	{
		_marker = p_marker;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
	}
}
