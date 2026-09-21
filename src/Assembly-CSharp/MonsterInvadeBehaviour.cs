using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

public class MonsterInvadeBehaviour : CharacterBehaviour
{
	public MonsterInvadeBehaviour()
	{
		base.priority = 900;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		Gathering currentGathering = character.gatheringComponent.currentGathering;
		if (!currentGathering.isWaitTimeOver)
		{
			character.jobComponent.TriggerRoamAroundStructure(out producedJob);
		}
		else if (currentGathering.target != null)
		{
			BaseSettlement currentSettlement = currentGathering.target.currentSettlement;
			if (character.gridTileLocation != null && currentSettlement != null)
			{
				if (character.gridTileLocation.IsPartOfSettlement(currentSettlement))
				{
					Character randomResidentForInvasionTargetThatIsInsideSettlement = currentSettlement.GetRandomResidentForInvasionTargetThatIsInsideSettlement(currentSettlement, character);
					if (randomResidentForInvasionTargetThatIsInsideSettlement != null)
					{
						character.combatComponent.Fight(randomResidentForInvasionTargetThatIsInsideSettlement, "Hostility");
					}
					else
					{
						character.jobComponent.TriggerRoamAroundStructure(out producedJob);
					}
				}
				else if (currentGathering.target is LocationStructure locationStructure)
				{
					LocationGridTile randomElement = CollectionUtilities.GetRandomElement(locationStructure.passableTiles);
					character.jobComponent.CreateGoToJob(randomElement, out producedJob);
				}
			}
			else
			{
				character.jobComponent.TriggerRoamAroundStructure(out producedJob);
			}
		}
		else
		{
			MonsterInvadeGathering monsterInvadeGathering = currentGathering as MonsterInvadeGathering;
			if (character.areaLocation == monsterInvadeGathering.targetArea)
			{
				Character randomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory = monsterInvadeGathering.targetArea.locationCharacterTracker.GetRandomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory(monsterInvadeGathering.targetArea);
				if (randomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory != null)
				{
					character.combatComponent.Fight(randomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory, "Hostility");
				}
				else
				{
					character.jobComponent.TriggerRoamAroundStructure(out producedJob);
				}
			}
			else
			{
				Area targetArea = monsterInvadeGathering.targetArea;
				LocationGridTile tile = targetArea.gridTileComponent.gridTiles[Random.Range(0, targetArea.gridTileComponent.gridTiles.Count)];
				character.jobComponent.TriggerRoamAroundStructure(out producedJob, tile);
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAGatheringJob(state: true);
		}
		return true;
	}
}
