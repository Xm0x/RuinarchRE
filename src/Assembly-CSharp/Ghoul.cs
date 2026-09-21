using System.Collections.Generic;
using UtilityScripts;

public class Ghoul : Summon
{
	public const string ClassName = "Ghoul";

	public override bool defaultDigMode => true;

	public Ghoul()
		: base(SUMMON_TYPE.Ghoul, "Ghoul", RACE.GHOUL, Utilities.GetRandomGender())
	{
	}

	public Ghoul(string className)
		: base(SUMMON_TYPE.Ghoul, className, RACE.GHOUL, Utilities.GetRandomGender())
	{
	}

	public Ghoul(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
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
			area.PopulateAreasInRange(list, 6, includeCenterTile: true);
			for (int i = 0; i < list.Count; i++)
			{
				Character randomCharacterInsideHexThatIsHumanoidOrSapientAndIsDeadAndIsNotInPrisonOrKennel = list[i].locationCharacterTracker.GetRandomCharacterInsideHexThatIsHumanoidOrSapientAndIsDeadAndIsNotInPrisonOrKennel();
				if (randomCharacterInsideHexThatIsHumanoidOrSapientAndIsDeadAndIsNotInPrisonOrKennel != null)
				{
					list2.Add(randomCharacterInsideHexThatIsHumanoidOrSapientAndIsDeadAndIsNotInPrisonOrKennel);
				}
			}
			if (list2.Count > 0)
			{
				character = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
			}
			RuinarchListPool<Area>.Release(list);
			RuinarchListPool<Character>.Release(list2);
			if (character != null)
			{
				base.jobComponent.TriggerRaiseCorpse(JOB_TYPE.AGITATED, character, out p_agitateJob);
				if (p_agitateJob is GoapPlanJob goapPlanJob)
				{
					goapPlanJob.SetIsAgitateJob(p_state: true);
				}
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
				return true;
			}
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Reanimate_Corpse_No_Target);
			return false;
		}
		return false;
	}
}
