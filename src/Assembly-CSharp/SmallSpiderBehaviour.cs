using System.Collections.Generic;
using UtilityScripts;

public class SmallSpiderBehaviour : BaseMonsterBehaviour
{
	public SmallSpiderBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (GameUtilities.RollChance(30))
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			PopulateWebbedCharactersAtHome(list, character);
			if (list.Count > 0)
			{
				Character randomElement = CollectionUtilities.GetRandomElement(list);
				RuinarchListPool<Character>.Release(list);
				return character.jobComponent.TriggerEatAlive(randomElement, out producedJob);
			}
			RuinarchListPool<Character>.Release(list);
		}
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob, checkIfPathPossibleWithoutDigging: true);
	}

	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
	}

	private void PopulateWebbedCharactersAtHome(List<Character> p_characterList, Character character)
	{
		if (character.homeStructure != null)
		{
			character.homeStructure.PopulateCharacterListThatIsWebbed(p_characterList);
		}
		else if (character.HasTerritory())
		{
			character.territory.locationCharacterTracker.PopulateCharacterListInsideHexThatHasTrait(p_characterList, "Webbed");
		}
	}
}
