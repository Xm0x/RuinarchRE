using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BuildMonsterSpawnerStructure : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public BuildMonsterSpawnerStructure()
		: base(INTERACTION_TYPE.BUILD_MONSTER_SPAWNER_STRUCTURE)
	{
		base.actionIconString = GoapActionStateDB.Build_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Build Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile)
		{
			return (otherData[0].obj as LocationGridTile).structure;
		}
		return base.GetTargetStructure(node);
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile)
		{
			return otherData[0].obj as LocationGridTile;
		}
		return null;
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			OtherData[] otherData = node.otherData;
			if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile locationGridTile)
			{
				if (node.actor.gridTileLocation != locationGridTile && !node.actor.gridTileLocation.IsNeighbour(locationGridTile))
				{
					goapActionInvalidity.isInvalid = true;
				}
				else
				{
					STRUCTURE_TYPE wildernessMonsterSpawnerStructureType = node.actor.behaviourComponent.wildernessMonsterSpawnerStructureType;
					if (wildernessMonsterSpawnerStructureType == STRUCTURE_TYPE.MONSTER_LAIR)
					{
						Area area = locationGridTile.area;
						if (area.IsPartOfVillage() || !area.elevationComponent.IsFully(ELEVATION.PLAIN) || area.structureComponent.HasStructureInArea() || area.gridTileComponent.HasCorruption())
						{
							goapActionInvalidity.isInvalid = true;
						}
					}
					else
					{
						StructureSetting structureSetting = new StructureSetting(wildernessMonsterSpawnerStructureType, RESOURCE.NONE);
						FACTION_TYPE p_factionType = FACTION_TYPE.Wild_Monsters;
						if (node.actor.faction != null)
						{
							p_factionType = node.actor.faction.factionType.type;
						}
						LocationStructureObject locationStructureObject = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(p_factionType, structureSetting)?.GetComponent<LocationStructureObject>();
						if (locationStructureObject == null || !locationStructureObject.HasEnoughSpaceIfPlacedOn(locationGridTile))
						{
							goapActionInvalidity.isInvalid = true;
						}
					}
				}
			}
		}
		return goapActionInvalidity;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode goapNode)
	{
		base.AddFillersToLog(log, goapNode);
		string text = goapNode.actor.behaviourComponent.wildernessMonsterSpawnerStructureType.LocalizedStructureName();
		string text2 = Utilities.GetArticleForWord(text) + " " + text;
		text2 = text2.Trim();
		log.AddToFillers(null, text2, LOG_IDENTIFIER.STRING_1);
	}

	public void AfterBuildSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		Character actor = goapNode.actor;
		LocationGridTile locationGridTile = otherData[0].obj as LocationGridTile;
		STRUCTURE_TYPE wildernessMonsterSpawnerStructureType = actor.behaviourComponent.wildernessMonsterSpawnerStructureType;
		StructureSetting structureSetting = new StructureSetting(wildernessMonsterSpawnerStructureType, RESOURCE.NONE);
		FACTION_TYPE p_factionType = FACTION_TYPE.Wild_Monsters;
		if (actor.faction != null)
		{
			p_factionType = actor.faction.factionType.type;
		}
		LocationStructureObject locationStructureObject = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(p_factionType, structureSetting)?.GetComponent<LocationStructureObject>();
		if (locationStructureObject == null || !locationStructureObject.HasEnoughSpaceIfPlacedOn(locationGridTile))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " overlap", goapNode.logTags);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
			actor.behaviourComponent.SetHasBuiltWildernessMonsterSpawnerStructure(p_state: true);
			return;
		}
		MonsterSpawner monsterSpawner = null;
		if (locationGridTile.tileObjectComponent.objHere is MonsterSpawner monsterSpawner2)
		{
			monsterSpawner = monsterSpawner2;
			actor.PickUpItem(monsterSpawner, changeCharacterOwnership: false, setOwnership: false);
		}
		Area area = locationGridTile.area;
		NPCSettlement p_settlement = LandmarkManager.Instance.CreateNewSettlement(area.region, LOCATION_TYPE.DUNGEON, area);
		LocationStructure dwelling = locationGridTile.tileObjectComponent.genericTileObject.InstantPlaceStructure(locationStructureObject.gameObject.name, p_settlement);
		area.areaItem.UpdatePathfindingGraph();
		goapNode.actor.MigrateHomeStructureTo(dwelling);
		if (monsterSpawner != null)
		{
			if (locationGridTile.tileObjectComponent.objHere != null)
			{
				locationGridTile.structure.RemovePOI(locationGridTile.tileObjectComponent.objHere);
			}
			monsterSpawner.SetStopProcessingOnPlacement(p_state: true);
			actor.DropItem(monsterSpawner, locationGridTile);
			monsterSpawner.SetStopProcessingOnPlacement(p_state: false);
		}
		actor.behaviourComponent.SetHasBuiltWildernessMonsterSpawnerStructure(p_state: true);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile { hasBlueprint: not false })
			{
				return false;
			}
			return poiTarget == actor;
		}
		return false;
	}
}
