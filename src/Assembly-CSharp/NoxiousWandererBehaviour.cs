using UnityEngine;

public class NoxiousWandererBehaviour : CharacterBehaviour
{
	public NoxiousWandererBehaviour()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (Random.Range(0, 100) < 15)
		{
			character.jobComponent.TriggerSpawnPoisonCloud(out producedJob);
		}
		else
		{
			character.jobComponent.TriggerRoamAroundTile(out producedJob);
		}
		return true;
	}
}
