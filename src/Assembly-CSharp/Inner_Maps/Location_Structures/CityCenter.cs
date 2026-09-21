using System.Collections.Generic;
using Maccima_Games.Util;

namespace Inner_Maps.Location_Structures;

public class CityCenter : ManMadeStructure
{
	public CityCenter(Region location)
		: base(STRUCTURE_TYPE.CITY_CENTER, location)
	{
		base.wallsAreMadeOf = WALL_RESOURCE.Wood;
	}

	public CityCenter(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		base.wallsAreMadeOf = WALL_RESOURCE.Wood;
	}

	protected override string GenerateName()
	{
		if (base.settlementLocation != null)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("location1", base.settlementLocation.name);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Structures_Table", "Village_Center", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			return localizedValue;
		}
		return base.structureType.LocalizedStructureName();
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		base.SubscribeListeners(shouldLock);
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted, shouldLock);
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
	}

	protected override void AfterSetSettlementLocation()
	{
		base.AfterSetSettlementLocation();
		if (GameManager.Instance.gameHasStarted)
		{
			ReGenerateName();
		}
	}

	public override bool CanBeDamagedByPlayerSpells()
	{
		return false;
	}

	public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource)
	{
	}

	public override void OnTileRepaired(LocationGridTile tile, int amount)
	{
	}

	public override bool DoesTileContributeToDamage(LocationGridTile tile)
	{
		return false;
	}

	private void OnDayStarted()
	{
		Area area = base.occupiedArea;
		LocationGridTile randomTileThatIsPassableAndHasNoObjectAndIsInWilderness = area.gridTileComponent.GetRandomTileThatIsPassableAndHasNoObjectAndIsInWilderness();
		if (randomTileThatIsPassableAndHasNoObjectAndIsInWilderness != null && area.tileObjectComponent.GetNumberOfTileObjects(TILE_OBJECT_TYPE.HERB_PLANT) < 4)
		{
			randomTileThatIsPassableAndHasNoObjectAndIsInWilderness.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HERB_PLANT), randomTileThatIsPassableAndHasNoObjectAndIsInWilderness);
		}
	}
}
