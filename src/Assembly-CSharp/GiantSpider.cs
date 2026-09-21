using System.Collections.Generic;
using UtilityScripts;

public class GiantSpider : SkinnableAnimal
{
	public const string ClassName = "Giant Spider";

	public override bool defaultDigMode => true;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.SPIDER_SILK;

	public GiantSpider()
		: base(SUMMON_TYPE.Giant_Spider, "Giant Spider", RACE.SPIDER, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public GiantSpider(string className)
		: base(SUMMON_TYPE.Giant_Spider, className, RACE.SPIDER, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public GiantSpider(SaveDataSkinnableAnimal data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetAvoidSettlements(state: true);
		base.movementComponent.SetEnableDigging(state: true);
	}

	public override void OnSummonAsPlayerMonster()
	{
		base.OnSummonAsPlayerMonster();
		base.movementComponent.SetAvoidSettlements(state: false);
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

	public override void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
		base.OnJobAddedToCharacterJobQueue(job, character);
		if (character == this && job.jobType == JOB_TYPE.MONSTER_ABDUCT && character.combatComponent.combatMode != COMBAT_MODE.Defend)
		{
			character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		}
	}

	public override void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		base.OnJobRemovedFromCharacterJobQueue(job, character, shouldBlacklist);
		if (character == this && job.jobType == JOB_TYPE.MONSTER_ABDUCT)
		{
			if (character is Summon summon)
			{
				character.combatComponent.SetCombatMode(summon.defaultCombatMode);
			}
			else
			{
				character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
			}
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
				Character randomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel = area2.locationCharacterTracker.GetRandomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel();
				if (randomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel != null)
				{
					list3.Add(randomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel);
					continue;
				}
				Character randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel = area2.locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel();
				if (randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel != null)
				{
					list2.Add(randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel);
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

	private void OnArachnophobiaToggled(bool p_isOn)
	{
		base.visuals.UpdateAllVisuals(this);
	}
}
