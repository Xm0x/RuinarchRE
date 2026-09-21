using System.Collections.Generic;
using UtilityScripts;

public class Tarantula : SkinnableAnimal
{
	public const string ClassName = "Tarantula";

	public override bool defaultDigMode => true;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.SPIDER_SILK;

	public Tarantula()
		: base(SUMMON_TYPE.Tarantula, "Tarantula", RACE.SPIDER, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public Tarantula(string className)
		: base(SUMMON_TYPE.Tarantula, className, RACE.SPIDER, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public Tarantula(SaveDataSkinnableAnimal data)
		: base(data)
	{
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<bool>(SettingsSignals.ARACHNOPHOBIA_TOGGLED, OnArachnophobiaToggled);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<bool>(SettingsSignals.ARACHNOPHOBIA_TOGGLED, OnArachnophobiaToggled);
		}
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
				Area area2 = list[i];
				Character randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel = area2.locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel();
				if (randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel != null)
				{
					list2.Add(randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel);
					continue;
				}
				Character randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel = area2.locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel();
				if (randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel != null)
				{
					list3.Add(randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel);
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
			RuinarchListPool<Character>.Release(list3);
			RuinarchListPool<Character>.Release(list2);
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

	private void OnArachnophobiaToggled(bool p_isOn)
	{
		base.visuals.UpdateAllVisuals(this);
	}
}
