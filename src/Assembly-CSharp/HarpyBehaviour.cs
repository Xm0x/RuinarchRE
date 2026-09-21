using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class HarpyBehaviour : BaseMonsterBehaviour
{
	public HarpyBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.currentStructure is Kennel)
		{
			return false;
		}
		if (character.IsAtHome())
		{
			if (GameUtilities.RollChance(1) && character.jobComponent.TryTriggerLayEgg(character, 4, TILE_OBJECT_TYPE.HARPY_EGG, out producedJob))
			{
				return true;
			}
			Harpy harpy = character as Harpy;
			if (!harpy.hasCapturedForTheDay)
			{
				harpy.SetHasCapturedForTheDay(state: true);
				if (ChanceData.RollChance(CHANCE_TYPE.Harpy_Capture) && TryCaptureCharacter(character, out producedJob))
				{
					return true;
				}
			}
			return character.jobComponent.TriggerRoamAroundTile(out producedJob);
		}
		if (character.HasHome())
		{
			return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
		}
		return false;
	}

	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob))
		{
			return true;
		}
		if (TryTakePersonalPatrolJob(p_character, 15, ref p_log, out p_producedJob))
		{
			return true;
		}
		if (GameUtilities.RollChance(1, ref p_log) && p_character.jobComponent.TryTriggerLayEgg(p_character, 5, TILE_OBJECT_TYPE.HARPY_EGG, out p_producedJob))
		{
			return true;
		}
		return p_character.jobComponent.TriggerRoamAroundTile(out p_producedJob);
	}

	private bool TryCaptureCharacter(Character actor, out JobQueueItem producedJob)
	{
		producedJob = null;
		Region currentRegion = actor.currentRegion;
		if (currentRegion != null)
		{
			Character targetForCapture = GetTargetForCapture(actor, currentRegion);
			if (targetForCapture != null)
			{
				LocationStructure destinationToDropCapturedCharacter = GetDestinationToDropCapturedCharacter(actor, currentRegion);
				if (destinationToDropCapturedCharacter != null)
				{
					return actor.jobComponent.TryTriggerCaptureCharacter(JOB_TYPE.CAPTURE_CHARACTER, targetForCapture, destinationToDropCapturedCharacter, out producedJob, doNotRecalculate: true);
				}
			}
		}
		return false;
	}

	private Character GetTargetForCapture(Character actor, Region region)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < region.charactersAtLocation.Count; i++)
		{
			Character character = region.charactersAtLocation[i];
			if (character != actor && character.race != actor.race && !character.isHidden && !character.isDead && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried())
			{
				list.Add(character);
			}
		}
		Character result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	private LocationStructure GetDestinationToDropCapturedCharacter(Character actor, Region region)
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		for (int i = 0; i < region.allSpecialStructures.Count; i++)
		{
			LocationStructure locationStructure = region.allSpecialStructures[i];
			if (locationStructure != actor.homeStructure && locationStructure.passableTiles.Count > 0)
			{
				list.Add(locationStructure);
			}
		}
		LocationStructure result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}
}
