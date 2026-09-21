using System.Collections.Generic;
using Locations.Settlements;
using UtilityScripts;

public class UnicornBehaviour : BaseMonsterBehaviour
{
	public UnicornBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.IsAtHome())
		{
			if (character.faction == FactionManager.Instance.wildMonsterFaction && ChanceData.RollChance(CHANCE_TYPE.Unicorn_Join_Faction, ref log))
			{
				Faction faction = null;
				List<Faction> list = RuinarchListPool<Faction>.Claim();
				for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
				{
					Faction faction2 = FactionManager.Instance.allFactions[i];
					if (faction2.isMajorNonPlayer && !faction2.isDisbanded && faction2.factionType.GetCrimeSeverity(null, null, CRIME_TYPE.Animal_Killing).IsConsideredACrime())
					{
						list.Add(faction2);
					}
				}
				if (list.Count > 0)
				{
					faction = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
				}
				RuinarchListPool<Faction>.Release(list);
				if (faction != null)
				{
					BaseSettlement baseSettlement = faction.GetRandomOwnedVillage();
					if (baseSettlement == null)
					{
						baseSettlement = faction.GetRandomOwnedSettlement();
					}
					character.ChangeFactionTo(faction, bypassIdeologyChecking: true);
					if (baseSettlement != null)
					{
						character.MigrateHomeTo(baseSettlement);
					}
					return true;
				}
			}
			return character.jobComponent.TriggerRoamAroundTile(out producedJob);
		}
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}
}
