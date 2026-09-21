using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class MonsterGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Placing_Monsters");
		yield return MapGenerator.Instance.StartCoroutine(LandmarkMonsterGeneration());
		yield return MapGenerator.Instance.StartCoroutine(CaveMonsterGeneration());
		yield return null;
	}

	private IEnumerator LandmarkMonsterGeneration()
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LandmarkManager.Instance.PopulateAllSpecialStructures(list);
		List<LocationGridTile> p_locationChoices = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < list.Count; i++)
		{
			LocationStructure locationStructure = list[i];
			if (locationStructure.structureType == STRUCTURE_TYPE.CAVE || locationStructure is AnimalDen || !GameUtilities.RollChance(70))
			{
				continue;
			}
			p_locationChoices.Clear();
			p_locationChoices.AddRange(locationStructure.passableTiles);
			MonsterMigrationBiomeAtomizedData randomMonsterToSpawn = LandmarkManager.Instance.GetStructureData(locationStructure.structureType).GetRandomMonsterToSpawn();
			if (randomMonsterToSpawn != null)
			{
				string debugLog = string.Empty;
				randomMonsterToSpawn.SpawnMonsters(ref p_locationChoices, locationStructure, ref debugLog);
				if (p_locationChoices.Count == 0)
				{
					break;
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(p_locationChoices);
		RuinarchListPool<LocationStructure>.Release(list);
		yield return null;
	}

	private IEnumerator CaveMonsterGeneration()
	{
		Region mainRegion = GridMap.Instance.mainRegion;
		if (mainRegion.HasStructure(STRUCTURE_TYPE.CAVE))
		{
			List<LocationStructure> structuresAtLocation = mainRegion.GetStructuresAtLocation(STRUCTURE_TYPE.CAVE);
			List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
			list.AddRange(structuresAtLocation.OrderByDescending((LocationStructure x) => x.tiles.Count));
			List<LocationGridTile> p_locationChoices = RuinarchListPool<LocationGridTile>.Claim();
			for (int num = 0; num < list.Count; num++)
			{
				LocationStructure locationStructure = list[num];
				Cave cave = locationStructure as Cave;
				if (cave.residents.Count <= 0 && cave.passableTiles.Count != 0 && !cave.hasConnectedMine && (WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Custom || !CharacterManager.Instance.GenerateRatmen(cave, GameUtilities.RandomBetweenTwoNumbers(1, 3), 8)))
				{
					p_locationChoices.Clear();
					p_locationChoices.AddRange(cave.passableTiles);
					MonsterMigrationBiomeAtomizedData randomMonsterToSpawn = LandmarkManager.Instance.GetStructureData(locationStructure.structureType).GetRandomMonsterToSpawn();
					string debugLog = string.Empty;
					randomMonsterToSpawn.SpawnMonsters(ref p_locationChoices, cave, ref debugLog);
					if (p_locationChoices.Count == 0)
					{
						Debug.LogWarning("Ran out of grid tiles to place monsters at structure " + cave.name);
						break;
					}
				}
			}
			RuinarchListPool<LocationGridTile>.Release(p_locationChoices);
			RuinarchListPool<LocationStructure>.Release(list);
		}
		yield return null;
	}
}
