using UnityEngine;

public class GolemBehaviour : BaseMonsterBehaviour
{
	public GolemBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character is Summon { gridTileLocation: not null } summon)
		{
			if ((summon.homeStructure == null || summon.homeStructure.hasBeenDestroyed) && !summon.HasTerritory())
			{
				summon.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character);
				if (summon.homeStructure == null && !summon.HasTerritory())
				{
					return summon.jobComponent.TriggerRoamAroundTile(out producedJob);
				}
				return true;
			}
			if (summon.isAtHomeStructure || summon.IsInTerritory())
			{
				bool flag = false;
				if (Random.Range(0, 100) < 35)
				{
					flag = summon.jobComponent.TriggerRoamAroundTerritory(out producedJob);
				}
				if (!flag)
				{
					summon.jobComponent.TriggerStand(out producedJob);
				}
				return true;
			}
			int num = Mathf.RoundToInt((float)summon.maxHP * 0.5f);
			if (summon.currentHP < num)
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
		return false;
	}
}
