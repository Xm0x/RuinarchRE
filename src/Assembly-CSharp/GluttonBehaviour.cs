using UnityEngine;

public class GluttonBehaviour : CharacterBehaviour
{
	public GluttonBehaviour()
	{
		base.priority = 15;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (Random.Range(0, 100) < 15 && character.needsComponent.isHungry)
		{
			character.needsComponent.PlanFullnessRecoveryGlutton(out producedJob);
			return true;
		}
		producedJob = null;
		return false;
	}
}
