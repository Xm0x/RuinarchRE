using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SocializingBehaviour : CharacterBehaviour
{
	public SocializingBehaviour()
	{
		base.priority = 800;
		attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[1] { BEHAVIOUR_COMPONENT_ATTRIBUTE.STOPS_BEHAVIOUR_LOOP };
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		LocationStructure targetSocializeStructure = character.behaviourComponent.targetSocializeStructure;
		if (character.behaviourComponent.socializingEndTime.hasValue && character.behaviourComponent.socializingEndTime.IsBefore(GameManager.Instance.Today()))
		{
			character.behaviourComponent.ClearOutSocializingBehaviour();
			producedJob = null;
			return false;
		}
		if (targetSocializeStructure == null || targetSocializeStructure.hasBeenDestroyed)
		{
			character.behaviourComponent.ClearOutSocializingBehaviour();
			producedJob = null;
			return false;
		}
		if (character.currentStructure == targetSocializeStructure)
		{
			if (targetSocializeStructure.structureType == STRUCTURE_TYPE.TAVERN)
			{
				if (GameUtilities.RollChance(15, ref log))
				{
					List<TileObject> tileObjectsOfType = targetSocializeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.TABLE);
					if (tileObjectsOfType != null)
					{
						for (int i = 0; i < tileObjectsOfType.Count; i++)
						{
							Table table = tileObjectsOfType[i] as Table;
							if (table.mapObjectState == MAP_OBJECT_STATE.BUILT && table.CanAccommodateCharacter(character) && character.jobComponent.TriggerDrinkJob(JOB_TYPE.SOCIALIZE, table, out producedJob))
							{
								return true;
							}
						}
					}
				}
			}
			else if (targetSocializeStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && GameUtilities.RollChance(10, ref log))
			{
				List<TileObject> tileObjectsOfType2 = targetSocializeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.WATER_WELL);
				if (tileObjectsOfType2 != null)
				{
					for (int j = 0; j < tileObjectsOfType2.Count; j++)
					{
						WaterWell waterWell = tileObjectsOfType2[j] as WaterWell;
						if (waterWell.mapObjectState == MAP_OBJECT_STATE.BUILT)
						{
							return character.jobComponent.TriggerDrinkWaterJob(waterWell, out producedJob);
						}
					}
				}
			}
			if (GameUtilities.RollChance(20, ref log) && ChatBehaviour(character, ref log, out producedJob))
			{
				return true;
			}
			if (character.jobComponent.TriggerRoamAroundStructure(JOB_TYPE.SOCIALIZE, out producedJob))
			{
				return true;
			}
			producedJob = null;
			return true;
		}
		LocationGridTile locationGridTile = ((targetSocializeStructure.passableTiles.Count > 0) ? CollectionUtilities.GetRandomElement(targetSocializeStructure.passableTiles) : CollectionUtilities.GetRandomElement(targetSocializeStructure.tiles));
		if (character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
		{
			return character.jobComponent.CreateGoToJob(JOB_TYPE.VISIT_STRUCTURE, locationGridTile, out producedJob);
		}
		character.behaviourComponent.ClearOutSocializingBehaviour();
		producedJob = null;
		return false;
	}

	private bool ChatBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.isNormalCharacter && character.hasMarker && character.marker.inVisionCharacters.Count > 0 && CharacterManager.Instance.HasCharacterNotConversedInMinutes(character, 9))
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			character.marker.PopulateCharactersThatIsNotDeadVillagerAndNotConversedInMinutes(list, 9);
			Character character2 = null;
			if (list.Count > 0)
			{
				character2 = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<Character>.Release(list);
			if (character2 != null)
			{
				if (character.nonActionEventsComponent.CanChat(character2) && GameUtilities.RollChance(50, ref log))
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, character2);
					producedJob = null;
					return true;
				}
				if (character.nonActionEventsComponent.CheckForFlirtTrigger(character, character2, isOnSight: false, ref log, out producedJob))
				{
					return true;
				}
			}
		}
		producedJob = null;
		return false;
	}
}
