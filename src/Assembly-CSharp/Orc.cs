using System.Collections.Generic;
using UtilityScripts;

public class Orc : Summon
{
	public const string ClassName = "Orc";

	public override bool defaultDigMode => true;

	public Orc()
		: base(SUMMON_TYPE.Orc, "Orc", RACE.ORC, Utilities.GetRandomGender())
	{
	}

	public Orc(string className)
		: base(SUMMON_TYPE.Orc, className, RACE.ORC, Utilities.GetRandomGender())
	{
	}

	public Orc(SaveDataSummon data)
		: base(data)
	{
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.homeStructure == null)
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Abduct_No_Home);
			return false;
		}
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		Area area = base.areaLocation;
		if (area != null)
		{
			Character character = null;
			List<Area> list = RuinarchListPool<Area>.Claim();
			List<Character> list2 = RuinarchListPool<Character>.Claim();
			List<Character> list3 = RuinarchListPool<Character>.Claim();
			area.PopulateAreasInRange(list, 6, includeCenterTile: true);
			for (int i = 0; i < list.Count; i++)
			{
				Character randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel = list[i].locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel();
				if (randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel != null)
				{
					if (!randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel.characterClass.IsCombatant())
					{
						list3.Add(randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel);
					}
					else
					{
						list2.Add(randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel);
					}
				}
			}
			if (list3.Count > 0)
			{
				character = list3[GameUtilities.RandomBetweenTwoNumbers(0, list3.Count - 1)];
			}
			else if (list2.Count > 0)
			{
				character = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
			}
			RuinarchListPool<Area>.Release(list);
			RuinarchListPool<Character>.Release(list2);
			RuinarchListPool<Character>.Release(list3);
			if (character != null)
			{
				base.jobComponent.TriggerMonsterAbduct(JOB_TYPE.MONSTER_ABDUCT, character, out p_agitateJob);
				if (p_agitateJob is GoapPlanJob goapPlanJob)
				{
					goapPlanJob.SetIsAgitateJob(p_state: true);
				}
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
				return true;
			}
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Abduct_Character_No_Target);
			return false;
		}
		return false;
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Abduct_Villager_Tooltip.ToStringEnum();
	}
}
