using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class HuntBeastBehaviour : CharacterBehaviour
{
	public HuntBeastBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working)
		{
			if (currentParty.targetDestination.IsAtTargetDestination(character))
			{
				LocationStructure targetStructure = (currentParty.currentQuest as HuntBeastPartyQuest).targetStructure;
				if (!targetStructure.hasBeenDestroyed)
				{
					if (targetStructure.GetFirstTileWithObject().tileObjectComponent.objHere is AnimalBurrow animalBurrow)
					{
						Summon randomAliveSpawnedMonsterFor = animalBurrow.GetRandomAliveSpawnedMonsterFor(character);
						if (randomAliveSpawnedMonsterFor != null)
						{
							character.combatComponent.Fight(randomAliveSpawnedMonsterFor, "Hostility");
							return true;
						}
						randomAliveSpawnedMonsterFor = animalBurrow.GetRandomDeadSpawnedMonsterForAnimalHaulRelativeTo(character);
						if (randomAliveSpawnedMonsterFor != null)
						{
							character.jobComponent.TryTriggerHaulAnimalCorpse(randomAliveSpawnedMonsterFor, out producedJob);
							if (producedJob != null)
							{
								producedJob.SetIsThisAPartyJob(state: true);
								return true;
							}
						}
						currentParty.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
						return true;
					}
					currentParty.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Structure_Destroyed"));
					return true;
				}
				currentParty.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Structure_Destroyed"));
				return true;
			}
			LocationGridTile randomPassableTile = currentParty.targetDestination.GetRandomPassableTile();
			character.jobComponent.CreatePartyGoToJob(randomPassableTile, out producedJob);
			if (producedJob != null)
			{
				producedJob.SetIsThisAPartyJob(state: true);
				return true;
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return false;
	}

	private Summon GetRandomDeadBeastToHaul(Area p_area)
	{
		Summon result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < p_area.locationCharacterTracker.charactersAtLocation.Count; i++)
		{
			Character character = p_area.locationCharacterTracker.charactersAtLocation[i];
			if (character.isDead && character.hasMarker && character is Summon summon && summon.summonType.IsAnimalBeast())
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as Summon;
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}
}
