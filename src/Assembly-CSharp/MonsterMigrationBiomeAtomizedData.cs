using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class MonsterMigrationBiomeAtomizedData
{
	[Tooltip("Mutually Exclusive with monsterType. This value will only be honored if monsterType is set to None.")]
	public string monsterClassName;

	public SUMMON_TYPE monsterType;

	public int minRange;

	public int maxRange;

	public int weight;

	public override string ToString()
	{
		return $"{monsterType} - Min: {minRange.ToString()} - Max: {maxRange.ToString()} - Weight: {weight.ToString()}";
	}

	public void SpawnMonsters(ref List<LocationGridTile> p_locationChoices, LocationStructure p_structure, ref string debugLog)
	{
		int num = GameUtilities.RandomBetweenTwoNumbers(minRange, maxRange);
		if (monsterType != SUMMON_TYPE.None)
		{
			for (int i = 0; i < num; i++)
			{
				if (p_locationChoices.Count == 0)
				{
					break;
				}
				Summon summon = CreateMonster(monsterType, p_locationChoices, p_structure, "", FactionManager.Instance.GetDefaultFactionForMonster(monsterType));
				p_locationChoices.Remove(summon.gridTileLocation);
			}
		}
		else
		{
			if (!(monsterClassName == "Ratman"))
			{
				return;
			}
			for (int j = 0; j < num; j++)
			{
				if (p_locationChoices.Count == 0)
				{
					break;
				}
				LocationGridTile randomElement = CollectionUtilities.GetRandomElement(p_locationChoices);
				CharacterManager.Instance.GenerateRatman(randomElement, p_structure);
				p_locationChoices.Remove(randomElement);
			}
		}
	}

	private Summon CreateMonster(SUMMON_TYPE summonType, List<LocationGridTile> locationChoices, LocationStructure homeStructure = null, string className = "", Faction faction = null)
	{
		LocationGridTile locationGridTile = ((homeStructure != null) ? CollectionUtilities.GetRandomElement(homeStructure.passableTiles) : CollectionUtilities.GetRandomElement(locationChoices));
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, faction ?? FactionManager.Instance.GetDefaultFactionForMonster(summonType), null, locationGridTile.parentMap.region, null, className);
		CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
		if (homeStructure != null)
		{
			summon.MigrateHomeStructureTo(homeStructure);
		}
		else if (locationGridTile.structure != null && locationGridTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && locationGridTile.structure.structureType != STRUCTURE_TYPE.OCEAN)
		{
			summon.MigrateHomeStructureTo(locationGridTile.structure);
		}
		else
		{
			summon.SetTerritory(locationGridTile.area, returnHome: false);
		}
		return summon;
	}
}
