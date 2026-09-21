using System.Collections.Generic;
using UtilityScripts;

public class MothmanBehaviour : BaseMonsterBehaviour
{
	public MothmanBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character is Mothman mothman)
		{
			bool flag = false;
			int num = 1;
			if (string.IsNullOrEmpty(mothman.stolenTraitName))
			{
				flag = true;
				num = 1;
			}
			else
			{
				flag = false;
				num = 2;
			}
			Area areaLocation = mothman.areaLocation;
			if (areaLocation != null && GameUtilities.RollChance(num, ref log))
			{
				TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
				if (currentTimeInWordsOfTick == TIME_IN_WORDS.LUNCH_TIME || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT)
				{
					Character character2 = null;
					List<Area> list = RuinarchListPool<Area>.Claim();
					areaLocation.PopulateAreasInRange(list, 4, includeCenterTile: true);
					List<Character> list2 = RuinarchListPool<Character>.Claim();
					for (int i = 0; i < list.Count; i++)
					{
						Character randomCharacterForMothmanStealOrGiveTrait = list[i].locationCharacterTracker.GetRandomCharacterForMothmanStealOrGiveTrait(mothman);
						if (randomCharacterForMothmanStealOrGiveTrait != null)
						{
							list2.Add(randomCharacterForMothmanStealOrGiveTrait);
						}
					}
					if (list2.Count > 0)
					{
						character2 = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
					}
					RuinarchListPool<Character>.Release(list2);
					RuinarchListPool<Area>.Release(list);
					if (character2 != null)
					{
						if (flag)
						{
							return mothman.jobComponent.TriggerStealTraitCharacter(JOB_TYPE.STEAL_GIVE_TRAIT, character2, out producedJob);
						}
						return mothman.jobComponent.TriggerGiveTraitCharacter(JOB_TYPE.STEAL_GIVE_TRAIT, character2, out producedJob);
					}
				}
			}
		}
		if (character.HasHome() && !character.IsAtHome() && character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob))
		{
			return true;
		}
		if (GameUtilities.RollChance(10))
		{
			character.jobComponent.TriggerPray(out producedJob);
			return true;
		}
		if (GameUtilities.RollChance(20))
		{
			return character.jobComponent.TriggerStand(out producedJob);
		}
		return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE, out producedJob);
	}
}
