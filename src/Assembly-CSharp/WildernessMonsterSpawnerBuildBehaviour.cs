using Inner_Maps;
using Inner_Maps.Location_Structures;

public class WildernessMonsterSpawnerBuildBehaviour : CharacterBehaviour
{
	public WildernessMonsterSpawnerBuildBehaviour()
	{
		base.priority = 50;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!character.behaviourComponent.hasBuiltWildernessMonsterSpawnerStructure)
		{
			TileObject tileObjectByPersistentIDSafe = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentIDSafe(character.behaviourComponent.monsterSpawnerID);
			if (tileObjectByPersistentIDSafe != null)
			{
				LocationGridTile gridTileLocation = tileObjectByPersistentIDSafe.gridTileLocation;
				if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
				{
					STRUCTURE_TYPE wildernessMonsterSpawnerStructureType = character.behaviourComponent.wildernessMonsterSpawnerStructureType;
					FACTION_TYPE p_factionType = FACTION_TYPE.Wild_Monsters;
					if (character is Summon summon)
					{
						p_factionType = FactionManager.Instance.GetDefaultFactionForMonster(summon.summonType).factionType.type;
					}
					StructureSetting structureSetting = new StructureSetting(wildernessMonsterSpawnerStructureType, RESOURCE.NONE);
					LocationStructureObject locationStructureObject = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(p_factionType, structureSetting)?.GetComponent<LocationStructureObject>();
					if (locationStructureObject != null && locationStructureObject.HasEnoughSpaceIfPlacedOn(gridTileLocation))
					{
						return character.jobComponent.TriggerBuildMonsterSpawnerStructure(gridTileLocation, out producedJob);
					}
					Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowerAlerts_Table", "Monster_Spawner_No_Space_Structure", LOG_TAG.Major);
					log2.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log2.AddToFillers(null, LocalizationManager.Instance.GetLocalizedValue("Structures_Table", wildernessMonsterSpawnerStructureType.ToStringEnum()), LOG_IDENTIFIER.STRING_1);
					log2.AddToFillers(tileObjectByPersistentIDSafe, tileObjectByPersistentIDSafe.nameplateName, LOG_IDENTIFIER.TARGET_CHARACTER);
					log2.AddLogToDatabase();
					PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
					character.behaviourComponent.RemoveBehaviourComponent(typeof(WildernessMonsterSpawnerBuildBehaviour));
					return true;
				}
			}
			return false;
		}
		character.behaviourComponent.RemoveBehaviourComponent(typeof(WildernessMonsterSpawnerBuildBehaviour));
		return true;
	}
}
