using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class WyvernEgg : MonsterEgg
{
	public WyvernEgg()
		: base(TILE_OBJECT_TYPE.WYVERN_EGG, SUMMON_TYPE.Wyvern, GameManager.Instance.GetTicksBasedOnHour(4))
	{
	}

	public WyvernEgg(SaveDataMonsterEgg data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return $"Wyvern Egg {base.id}";
	}

	protected override void Hatch()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		LocationStructure homeStructure = null;
		Area territory = locationGridTile?.area;
		Faction faction = null;
		BaseSettlement homeSettlement = null;
		bool flag = false;
		LocationStructure locationStructure = locationGridTile?.structure;
		if (locationStructure != null && locationStructure is WyvernCoop && locationStructure.settlementLocation != null)
		{
			homeSettlement = locationStructure.settlementLocation;
			faction = locationStructure.settlementLocation.owner;
			homeStructure = locationStructure;
			flag = true;
		}
		if (!flag)
		{
			ProcessHomeOfHatchedEggs(ref homeStructure, ref territory, ref homeSettlement);
			if (homeSettlement == null)
			{
				homeSettlement = homeStructure?.settlementLocation;
			}
		}
		if (faction == null)
		{
			faction = FactionManager.Instance.GetDefaultFactionForMonster(SUMMON_TYPE.Wyvern);
		}
		Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Wyvernling, faction, homeSettlement, GridMap.Instance.mainRegion, homeStructure, "", bypassIdeologyChecking: true);
		CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
		if (!summon.HasHome())
		{
			summon.SetTerritory(territory);
		}
	}
}
