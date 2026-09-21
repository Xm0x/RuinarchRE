using Traits;
using UnityEngine;

public class PsychopathBehaviour : CharacterBehaviour
{
	public PsychopathBehaviour()
	{
		base.priority = 12;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (Random.Range(0, 100) < 15)
		{
			Psychopath traitOrStatus = character.traitContainer.GetTraitOrStatus<Psychopath>("Psychopath");
			traitOrStatus.CheckTargetVictimIfStillAvailable();
			if (traitOrStatus.targetVictim != null && (traitOrStatus.targetVictim.isAtHomeStructure || traitOrStatus.targetVictim.IsInHomeSettlement()) && traitOrStatus.CreateHuntVictimJob(out producedJob))
			{
				return true;
			}
		}
		return false;
	}
}
