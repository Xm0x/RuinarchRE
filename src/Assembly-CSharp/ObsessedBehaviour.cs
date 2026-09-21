using Inner_Maps;
using Traits;

public class ObsessedBehaviour : CharacterBehaviour
{
	public ObsessedBehaviour()
	{
		base.priority = 30;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		Obsessed traitOrStatus = character.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
		if (traitOrStatus != null && traitOrStatus.targetCharacter != null)
		{
			Character targetCharacter = traitOrStatus.targetCharacter;
			if (targetCharacter.isDead && !targetCharacter.traitContainer.HasTrait("Mummified") && targetCharacter.grave == null)
			{
				LocationGridTile gridTileLocation = targetCharacter.gridTileLocation;
				LocationGridTile gridTileLocation2 = character.gridTileLocation;
				if (targetCharacter.hasMarker && gridTileLocation != null && !targetCharacter.isBeingSeized && targetCharacter.carryComponent.IsNotBeingCarried() && character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation) && gridTileLocation2 != null && gridTileLocation2.area.IsNearbyTo(gridTileLocation.area))
				{
					return character.jobComponent.CreateGoToSpecificTileJob(gridTileLocation);
				}
			}
			if (!character.traitContainer.HasTrait("Euphoric") && targetCharacter.homeStructure != null)
			{
				if (character.currentStructure == targetCharacter.homeStructure)
				{
					int numberOfAliveAndCanWitnessSapientsHere = targetCharacter.homeStructure.GetNumberOfAliveAndCanWitnessSapientsHere();
					if (targetCharacter.isAtHomeStructure && targetCharacter.traitContainer.HasTrait("Resting"))
					{
						if (numberOfAliveAndCanWitnessSapientsHere < 3)
						{
							WeightedDictionary<INTERACTION_TYPE> weightedDictionary = new WeightedDictionary<INTERACTION_TYPE>();
							weightedDictionary.AddElement(INTERACTION_TYPE.WATCH_SLEEP, ChanceData.GetChance(CHANCE_TYPE.Watch_Sleep_Weight));
							weightedDictionary.AddElement(INTERACTION_TYPE.SNIFF_CLOTHES, ChanceData.GetChance(CHANCE_TYPE.Sniff_Clothes_Weight));
							weightedDictionary.AddElement(INTERACTION_TYPE.SMELL_HAIR, ChanceData.GetChance(CHANCE_TYPE.Smell_Hair_Weight));
							INTERACTION_TYPE actionType = weightedDictionary.PickRandomElementGivenWeights();
							return character.jobComponent.TriggerIdleJob(actionType, targetCharacter);
						}
					}
					else if (numberOfAliveAndCanWitnessSapientsHere == 1)
					{
						TileObject randomOwnedItemInHomeStructure = targetCharacter.GetRandomOwnedItemInHomeStructure();
						if (randomOwnedItemInHomeStructure != null)
						{
							WeightedDictionary<INTERACTION_TYPE> weightedDictionary2 = new WeightedDictionary<INTERACTION_TYPE>();
							weightedDictionary2.AddElement(INTERACTION_TYPE.LICK_TILE_OBJECT, ChanceData.GetChance(CHANCE_TYPE.Lick_Object_Weight));
							weightedDictionary2.AddElement(INTERACTION_TYPE.RUB_TILE_OBJECT, ChanceData.GetChance(CHANCE_TYPE.Rub_Object_Weight));
							INTERACTION_TYPE actionType2 = weightedDictionary2.PickRandomElementGivenWeights();
							return character.jobComponent.TriggerIdleJob(actionType2, randomOwnedItemInHomeStructure);
						}
					}
				}
				else if (ChanceData.RollChance(CHANCE_TYPE.Obsessed_Visit, ref log))
				{
					character.PlanFixedJob(JOB_TYPE.VISIT_STRUCTURE, INTERACTION_TYPE.VISIT, character, out producedJob, new OtherData[1]
					{
						new LocationStructureOtherData(targetCharacter.homeStructure)
					});
					return true;
				}
			}
		}
		return false;
	}
}
