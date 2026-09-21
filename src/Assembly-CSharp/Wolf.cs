using System.Collections.Generic;
using UtilityScripts;

public class Wolf : SkinnableAnimal
{
	public override bool defaultDigMode => true;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.WOLF_HIDE;

	public Wolf()
		: base(SUMMON_TYPE.Wolf, "Ravager", RACE.WOLF, Utilities.GetRandomGender())
	{
	}

	public Wolf(string className)
		: base(SUMMON_TYPE.Wolf, className, RACE.WOLF, Utilities.GetRandomGender())
	{
	}

	public Wolf(SaveDataSkinnableAnimal data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetEnableDigging(state: true);
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
			List<Character> list3 = RuinarchListPool<Character>.Claim();
			area.PopulateAreasInRange(list, 6, includeCenterTile: true);
			for (int i = 0; i < list.Count; i++)
			{
				Area area2 = list[i];
				Character randomCharacterInsideAreaThatIsAliveNonCombatantAndNotInPrisonOrKennel = area2.locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveNonCombatantAndNotInPrisonOrKennel();
				if (randomCharacterInsideAreaThatIsAliveNonCombatantAndNotInPrisonOrKennel != null)
				{
					list2.Add(randomCharacterInsideAreaThatIsAliveNonCombatantAndNotInPrisonOrKennel);
					continue;
				}
				Character randomCharacterInsideAreaThatIsAliveCombatantAndNotInPrisonOrKennel = area2.locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveCombatantAndNotInPrisonOrKennel();
				if (randomCharacterInsideAreaThatIsAliveCombatantAndNotInPrisonOrKennel != null)
				{
					list3.Add(randomCharacterInsideAreaThatIsAliveCombatantAndNotInPrisonOrKennel);
				}
			}
			if (list2.Count > 0)
			{
				character = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
			}
			else if (list3.Count > 0)
			{
				character = list3[GameUtilities.RandomBetweenTwoNumbers(0, list3.Count - 1)];
			}
			RuinarchListPool<Area>.Release(list);
			RuinarchListPool<Character>.Release(list2);
			RuinarchListPool<Character>.Release(list3);
			if (character == null)
			{
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Hunt_No_Target);
				return false;
			}
			base.jobComponent.TriggerAttackVillager(JOB_TYPE.AGITATED, character, out p_agitateJob);
			if (p_agitateJob is GoapPlanJob goapPlanJob)
			{
				goapPlanJob.SetIsAgitateJob(p_state: true);
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
				return true;
			}
		}
		return false;
	}
}
