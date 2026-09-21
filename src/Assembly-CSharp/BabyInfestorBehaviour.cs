using UnityEngine;

public class BabyInfestorBehaviour : CharacterBehaviour
{
	public BabyInfestorBehaviour()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character is Summon { gridTileLocation: not null } summon)
		{
			if ((summon.homeStructure == null || summon.homeStructure.hasBeenDestroyed) && !summon.HasTerritory())
			{
				summon.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, summon);
				if (summon.homeStructure == null && !summon.HasTerritory())
				{
					return summon.jobComponent.TriggerRoamAroundTile(out producedJob);
				}
				return true;
			}
			if (summon.isAtHomeStructure || summon.IsInTerritory())
			{
				bool flag = false;
				int num = Mathf.RoundToInt((float)summon.maxHP * 0.5f);
				if (summon.currentHP < num)
				{
					flag = summon.jobComponent.TriggerMonsterSleep(out producedJob);
				}
				else
				{
					int num2 = Random.Range(0, 100);
					if (num2 < 35)
					{
						flag = summon.jobComponent.TriggerRoamAroundTerritory(out producedJob);
					}
					else
					{
						TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
						if (currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
						{
							Random.Range(0, 100);
							if (num2 < 40)
							{
								flag = summon.jobComponent.TriggerMonsterSleep(out producedJob);
							}
						}
						else if (Random.Range(0, 100) < 5)
						{
							flag = summon.jobComponent.TriggerMonsterSleep(out producedJob);
						}
					}
				}
				if (!flag)
				{
					summon.jobComponent.TriggerStand(out producedJob);
				}
				return true;
			}
			int num3 = Mathf.RoundToInt((float)summon.maxHP * 0.5f);
			if (summon.currentHP < num3)
			{
				if (summon.homeStructure != null || summon.HasTerritory())
				{
					return summon.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
				}
			}
			else
			{
				if (Random.Range(0, 100) < 50)
				{
					summon.jobComponent.TriggerRoamAroundTile(out producedJob);
					return true;
				}
				if (summon.homeStructure != null || summon.HasTerritory())
				{
					return summon.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
				}
			}
		}
		return true;
	}
}
