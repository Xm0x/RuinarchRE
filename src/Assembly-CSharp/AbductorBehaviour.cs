using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class AbductorBehaviour : CharacterBehaviour
{
	public AbductorBehaviour()
	{
		base.priority = 30;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if (character.behaviourComponent.IsNestBlocked(out var blocker))
		{
			return character.jobComponent.TriggerDestroy(blocker, out producedJob, "Destroy_Blocker");
		}
		if (character.behaviourComponent.AlreadyHasAbductedVictimAtNest(out var target))
		{
			if (target.isDead)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, target, character);
				goapPlanJob.SetCannotBePushedBack(state: true);
				LocationGridTile firstNearestTileFromThisWithNoObject = character.behaviourComponent.nest.GetFirstNearestTileFromThisWithNoObject(thisStructureOnly: false, character.behaviourComponent.nest);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[2] { firstNearestTileFromThisWithNoObject.structure, firstNearestTileFromThisWithNoObject });
				producedJob = goapPlanJob;
				return true;
			}
			bool flag = false;
			switch (currentTimeInWordsOfTick)
			{
			case TIME_IN_WORDS.MORNING:
			case TIME_IN_WORDS.AFTERNOON:
			case TIME_IN_WORDS.LUNCH_TIME:
				flag = !character.behaviourComponent.hasEatenInTheMorning;
				break;
			case TIME_IN_WORDS.EARLY_NIGHT:
			case TIME_IN_WORDS.LATE_NIGHT:
				flag = !character.behaviourComponent.hasEatenInTheNight;
				break;
			}
			if (flag && GameUtilities.RollChance(40))
			{
				return character.jobComponent.TriggerEatAlive(target, out producedJob);
			}
			return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
		}
		if (currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
		{
			Character randomValidAbductTarget = GetRandomValidAbductTarget(character);
			if (randomValidAbductTarget != null)
			{
				return character.jobComponent.TriggerMonsterAbduct(randomValidAbductTarget, out producedJob, character.behaviourComponent.nest);
			}
			return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
		}
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.movementComponent.SetEnableDigging(state: true);
		character.behaviourComponent.OnBecomeAbductor();
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.movementComponent.SetEnableDigging(state: false);
		character.behaviourComponent.OnNoLongerAbductor();
	}

	public override void OnLoadBehaviourToCharacter(Character character)
	{
		base.OnLoadBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeAbductor();
	}

	private Character GetRandomValidAbductTarget(Character abductor)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		Area areaLocation = abductor.areaLocation;
		if (areaLocation != null)
		{
			List<Area> list2 = RuinarchListPool<Area>.Claim();
			areaLocation.PopulateAreasInRange(list2, 6, includeCenterTile: true);
			for (int i = 0; i < list2.Count; i++)
			{
				Area area = list2[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character = area.locationCharacterTracker.charactersAtLocation[j];
					LocationStructure currentStructure = character.currentStructure;
					if ((character is Animal || (character.isNormalCharacter && !character.isDead && character.traitContainer.HasTrait("Resting") && !(currentStructure is DemonicStructure))) && !character.isAlliedWithPlayer && character.gridTileLocation != null)
					{
						list.Add(character);
					}
				}
			}
			RuinarchListPool<Area>.Release(list2);
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}
}
