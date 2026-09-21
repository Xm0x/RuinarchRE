using System.Collections.Generic;
using UtilityScripts;

public class DefaultExtraCatcher : CharacterBehaviour
{
	public DefaultExtraCatcher()
	{
		base.priority = 0;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.isNormalCharacter && character.hasMarker && character.marker.inVisionCharacters.Count > 0 && CharacterManager.Instance.HasCharacterNotConversedInMinutes(character, 6))
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			character.marker.PopulateCharactersThatIsNotDeadVillagerAndNotConversedInMinutes(list, 6);
			Character character2 = null;
			if (list.Count > 0)
			{
				character2 = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<Character>.Release(list);
			if (character2 != null)
			{
				if (character.nonActionEventsComponent.CanChat(character2) && GameUtilities.RollChance(20, ref log))
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, character2);
					return true;
				}
				if (character.nonActionEventsComponent.CheckForFlirtTrigger(character, character2, isOnSight: false, ref log, out producedJob))
				{
					return true;
				}
			}
		}
		return character.jobComponent.TriggerStand(out producedJob);
	}
}
