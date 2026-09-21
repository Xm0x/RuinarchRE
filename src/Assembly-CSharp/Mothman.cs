using System;
using System.Collections.Generic;
using UtilityScripts;

public class Mothman : Summon
{
	public const string ClassName = "Mothman";

	public override bool defaultDigMode => true;

	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;

	public override Type serializedData => typeof(SaveDataMothman);

	public string stolenTraitName { get; private set; }

	public Mothman()
		: base(SUMMON_TYPE.Mothman, "Mothman", RACE.MOTHMAN, Utilities.GetRandomGender())
	{
		stolenTraitName = string.Empty;
	}

	public Mothman(string className)
		: base(SUMMON_TYPE.Mothman, className, RACE.MOTHMAN, Utilities.GetRandomGender())
	{
		stolenTraitName = string.Empty;
	}

	public Mothman(SaveDataMothman data)
		: base(data)
	{
		stolenTraitName = data.stolenTraitName;
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetToFlying();
	}

	public override void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
		base.OnJobAddedToCharacterJobQueue(job, character);
		if (character == this && (job.jobType == JOB_TYPE.STEAL_GIVE_TRAIT || job.jobType == JOB_TYPE.AGITATED) && character.combatComponent.combatMode != COMBAT_MODE.Passive)
		{
			character.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
		}
	}

	public override void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		base.OnJobRemovedFromCharacterJobQueue(job, character, shouldBlacklist);
		if (character == this && (job.jobType == JOB_TYPE.STEAL_GIVE_TRAIT || job.jobType == JOB_TYPE.AGITATED))
		{
			character.combatComponent.SetCombatMode(defaultCombatMode);
		}
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		bool flag = false;
		flag = (string.IsNullOrEmpty(stolenTraitName) ? true : false);
		Area area = base.areaLocation;
		if (area != null)
		{
			Character character = null;
			List<Area> list = RuinarchListPool<Area>.Claim();
			area.PopulateAreasInRange(list, 4, includeCenterTile: true);
			List<Character> list2 = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < list.Count; i++)
			{
				Character randomCharacterForMothmanStealOrGiveTrait = list[i].locationCharacterTracker.GetRandomCharacterForMothmanStealOrGiveTrait(this);
				if (randomCharacterForMothmanStealOrGiveTrait != null)
				{
					list2.Add(randomCharacterForMothmanStealOrGiveTrait);
				}
			}
			if (list2.Count > 0)
			{
				character = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
			}
			RuinarchListPool<Character>.Release(list2);
			RuinarchListPool<Area>.Release(list);
			if (character != null)
			{
				if (flag)
				{
					base.jobComponent.TriggerStealTraitCharacter(JOB_TYPE.AGITATED, character, out p_agitateJob);
					if (p_agitateJob is GoapPlanJob goapPlanJob)
					{
						goapPlanJob.SetIsAgitateJob(p_state: true);
						CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
						return true;
					}
				}
				else
				{
					base.jobComponent.TriggerGiveTraitCharacter(JOB_TYPE.AGITATED, character, out p_agitateJob);
					if (p_agitateJob is GoapPlanJob goapPlanJob2)
					{
						goapPlanJob2.SetIsAgitateJob(p_state: true);
						CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
						return true;
					}
				}
			}
			else if (flag)
			{
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_1);
			}
			else
			{
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_2);
			}
		}
		return false;
	}

	public void SetStolenTraitName(string p_traitName)
	{
		stolenTraitName = p_traitName;
	}
}
