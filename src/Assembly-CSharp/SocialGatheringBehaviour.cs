using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class SocialGatheringBehaviour : CharacterBehaviour
{
	public SocialGatheringBehaviour()
	{
		base.priority = 450;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		Gathering currentGathering = character.gatheringComponent.currentGathering;
		if (!currentGathering.isWaitTimeOver)
		{
			if (character.currentStructure == currentGathering.target)
			{
				if (character.previousCharacterDataComponent.previousJobType == JOB_TYPE.PARTY_GO_TO)
				{
					result = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
				}
				else
				{
					int num = Random.Range(0, 100);
					if (num < 15)
					{
						result = character.jobComponent.TriggerSingJob(out producedJob);
					}
					else if (num >= 15 && num < 30)
					{
						result = character.jobComponent.TriggerDanceJob(out producedJob);
					}
					else if (num >= 30 && num < 40)
					{
						TileObject unoccupiedTileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.TABLE);
						if (unoccupiedTileObject != null)
						{
							result = character.jobComponent.TriggerPartyDrinkJob(unoccupiedTileObject as Table, out producedJob);
						}
					}
					else if (num >= 40 && num < 50)
					{
						TileObject unoccupiedTileObject2 = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK);
						if (unoccupiedTileObject2 != null)
						{
							result = character.jobComponent.TriggerPlayCardsJob(unoccupiedTileObject2 as Desk, out producedJob);
						}
					}
					else if (num >= 50 && num < 70)
					{
						Character randomCharacterThatIsAliveCanPerformAndWitnessAndNotInCombatExcept = character.currentStructure.GetRandomCharacterThatIsAliveCanPerformAndWitnessAndNotInCombatExcept(character);
						if (randomCharacterThatIsAliveCanPerformAndWitnessAndNotInCombatExcept != null && character.nonActionEventsComponent.CanChat(randomCharacterThatIsAliveCanPerformAndWitnessAndNotInCombatExcept))
						{
							result = character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, randomCharacterThatIsAliveCanPerformAndWitnessAndNotInCombatExcept);
						}
					}
					else if (num >= 70 && num < 85)
					{
						TileObject unoccupiedTileObject3 = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK);
						if (unoccupiedTileObject3 != null)
						{
							result = character.jobComponent.TriggerPlayCardsJob(unoccupiedTileObject3 as Desk, out producedJob);
						}
					}
					else
					{
						result = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
					}
				}
			}
			else if (currentGathering.target is LocationStructure locationStructure)
			{
				LocationGridTile randomElement = CollectionUtilities.GetRandomElement(locationStructure.passableTiles);
				result = character.jobComponent.CreatePartyGoToJob(randomElement, out producedJob);
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAGatheringJob(state: true);
		}
		return result;
	}
}
