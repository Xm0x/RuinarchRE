using UnityEngine;

public class DefaultOutside : CharacterBehaviour
{
	public DefaultOutside()
	{
		base.priority = 4;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (!character.currentStructure.isInterior)
		{
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
			if ((currentTimeInWordsOfTick == TIME_IN_WORDS.MORNING || currentTimeInWordsOfTick == TIME_IN_WORDS.LUNCH_TIME || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTERNOON) && Random.Range(0, 100) < 25)
			{
				character.jobComponent.PlanIdleStrollOutside(out producedJob);
				return true;
			}
			if ((character.homeStructure != null && !character.homeStructure.hasBeenDestroyed) || character.HasTerritory())
			{
				return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
			}
			return character.jobComponent.TriggerStand(out producedJob);
		}
		producedJob = null;
		return false;
	}
}
