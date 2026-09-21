using System;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

public class SpiderEgg : MonsterEgg
{
	public override Type serializedData => typeof(SaveDataSpiderEgg);

	public SpiderEgg()
		: base(TILE_OBJECT_TYPE.SPIDER_EGG, SUMMON_TYPE.Giant_Spider, GameManager.Instance.GetTicksBasedOnHour(1))
	{
	}

	public SpiderEgg(SaveDataSpiderEgg data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Spider Egg " + base.id;
	}

	protected override void Hatch()
	{
		LocationStructure homeStructure = null;
		Area territory = null;
		BaseSettlement homeSettlement = null;
		ProcessHomeOfHatchedEggs(ref homeStructure, ref territory, ref homeSettlement);
		if (homeSettlement == null)
		{
			homeSettlement = homeStructure?.settlementLocation;
		}
		if (territory == null)
		{
			territory = gridTileLocation?.area;
		}
		int num = 0;
		if (homeSettlement != null)
		{
			num = homeSettlement.GetNumberOfResidentsForSpiderEggHatching();
		}
		else if (homeStructure != null)
		{
			num = homeStructure.GetNumberOfResidentsForSpiderEggHatching();
		}
		else if (territory != null)
		{
			num = GridMap.Instance.mainRegion.GetCountOfAliveCharacterForSpiderEggHatching(territory);
		}
		bool num2 = num < 4;
		int num3 = 0;
		if (num2)
		{
			num3 = UnityEngine.Random.Range(2, 4);
		}
		for (int i = 0; i < num3; i++)
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Small_Spider, base.faction, homeSettlement, GridMap.Instance.mainRegion, homeStructure, "", bypassIdeologyChecking: true);
			CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
			if (!summon.HasHome())
			{
				summon.SetTerritory(territory);
			}
		}
	}
}
