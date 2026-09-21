using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using Locations.Settlements;
using Traits;
using UnityEngine;
using UtilityScripts;

public class WolfBehaviour : BaseMonsterBehaviour
{
	public WolfBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory())
		{
			List<BaseSettlement> list = null;
			for (int i = 0; i < character.currentRegion.settlementsInRegion.Count; i++)
			{
				BaseSettlement baseSettlement = character.currentRegion.settlementsInRegion[i];
				if (baseSettlement is NPCSettlement && baseSettlement.HasResidentWithRace(RACE.WOLF, character))
				{
					if (list == null)
					{
						list = new List<BaseSettlement>();
					}
					list.Add(baseSettlement);
				}
			}
			if (list != null)
			{
				LocationStructure randomStructureThatCharacterCanBeResidentAndIsNot = CollectionUtilities.GetRandomElement(list).GetRandomStructureThatCharacterCanBeResidentAndIsNot(character, STRUCTURE_TYPE.WILDERNESS);
				if (randomStructureThatCharacterCanBeResidentAndIsNot != null)
				{
					character.MigrateHomeStructureTo(randomStructureThatCharacterCanBeResidentAndIsNot);
					return true;
				}
			}
			else
			{
				List<LocationStructure> structuresAtLocation = character.currentRegion.GetStructuresAtLocation(STRUCTURE_TYPE.MONSTER_LAIR);
				List<LocationStructure> list2 = RuinarchListPool<LocationStructure>.Claim();
				if (structuresAtLocation != null)
				{
					for (int j = 0; j < structuresAtLocation.Count; j++)
					{
						LocationStructure locationStructure = structuresAtLocation[j];
						if (locationStructure.CanBeResidentHere(character))
						{
							list2.Add(locationStructure);
						}
					}
				}
				LocationStructure locationStructure2 = null;
				if (list2.Count > 0)
				{
					locationStructure2 = CollectionUtilities.GetRandomElement(list2);
				}
				RuinarchListPool<LocationStructure>.Release(list2);
				if (locationStructure2 != null)
				{
					character.MigrateHomeStructureTo(locationStructure2);
					return true;
				}
			}
			Area area = null;
			Area areaLocation = character.areaLocation;
			if (areaLocation != null && areaLocation.elevationComponent.IsFully(ELEVATION.PLAIN) && !areaLocation.structureComponent.HasStructureInArea() && !areaLocation.IsNextToOrPartOfVillage())
			{
				area = areaLocation;
			}
			if (area == null)
			{
				area = GetNoStructurePlainAreaInRegion(character.currentRegion);
			}
			if (area == null)
			{
				area = GetNoStructurePlainAreaInAllRegions();
			}
			LocationGridTile centerGridTile = area.gridTileComponent.centerGridTile;
			character.jobComponent.TriggerSpawnWolfLair(centerGridTile, out producedJob);
			return true;
		}
		if (Utilities.IsEven(GameManager.Instance.Today().day) && GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 6 && Random.Range(0, 2) == 1)
		{
			Area areaThatIsNearbyWithFeatureThatIsNearestTo = character.currentRegion.GetAreaThatIsNearbyWithFeatureThatIsNearestTo(AreaFeatureDB.Game_Feature, character);
			if (areaThatIsNearbyWithFeatureThatIsNearestTo != null)
			{
				Hunting hunting = TraitManager.Instance.CreateNewInstancedTraitClass<Hunting>("Hunting");
				hunting.SetTargetArea(areaThatIsNearbyWithFeatureThatIsNearestTo);
				character.traitContainer.AddTrait(character, hunting);
				return true;
			}
			return false;
		}
		return false;
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.AddAdvertisedAction(INTERACTION_TYPE.BUILD_WOLF_LAIR);
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.RemoveAdvertisedAction(INTERACTION_TYPE.BUILD_WOLF_LAIR);
	}

	private Area GetNoStructurePlainAreaInAllRegions()
	{
		return GetNoStructurePlainAreaInRegion(GridMap.Instance.mainRegion);
	}

	private Area GetNoStructurePlainAreaInRegion(Region region)
	{
		return region.GetRandomAreaThatIsUncorruptedFullyPlainNoStructureAndNotNextToOrPartOfVillage();
	}
}
