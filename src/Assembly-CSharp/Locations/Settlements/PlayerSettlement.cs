using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Locations.Settlements;

public class PlayerSettlement : BaseSettlement
{
	public List<LocationGridTile> corruptedTiles { get; private set; }

	public override Type serializedData => typeof(SaveDataPlayerSettlement);

	public PlayerSettlement()
		: base(LOCATION_TYPE.DEMONIC_INTRUSION)
	{
		corruptedTiles = new List<LocationGridTile>(100);
	}

	public PlayerSettlement(SaveDataPlayerSettlement saveDataBaseSettlement)
		: base(saveDataBaseSettlement)
	{
		corruptedTiles = new List<LocationGridTile>(100);
	}

	public void SubscribeToListeners()
	{
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	public void UnsubscribeFromListeners()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		RemoveResident(p_character);
	}

	public override void AssignCharacterToDwellingInArea(Character character, LocationStructure dwellingOverride = null)
	{
		if (base.structures == null || (!character.isFactionless && !base.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)))
		{
			return;
		}
		if (character.isFactionless)
		{
			character.SetHomeStructure(null);
			return;
		}
		LocationStructure locationStructure = dwellingOverride;
		if (locationStructure == null && PlayerManager.Instance != null && PlayerManager.Instance.player != null && base.id == PlayerManager.Instance.player.playerSettlement.id)
		{
			locationStructure = base.structures[STRUCTURE_TYPE.DWELLING][0];
		}
		if (locationStructure == null)
		{
			locationStructure = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		}
		character.ChangeHomeStructure(locationStructure);
	}

	protected override bool IsResidentsFull()
	{
		return false;
	}

	public void AddPortalAreaToPlayerSettlement(Area p_area)
	{
		if (!HasArea(p_area))
		{
			base.areas.Add(p_area);
			p_area.AddSettlementOnArea(this);
		}
	}

	public LocationStructure GetRandomStructureInRegion(Region region)
	{
		List<LocationStructure> list = null;
		for (int i = 0; i < base.allStructures.Count; i++)
		{
			LocationStructure locationStructure = base.allStructures[i];
			if (locationStructure.region == region)
			{
				if (list == null)
				{
					list = new List<LocationStructure>();
				}
				list.Add(locationStructure);
			}
		}
		if (list != null)
		{
			return CollectionUtilities.GetRandomElement(list);
		}
		return null;
	}

	public override void LoadReferences(SaveDataBaseSettlement data)
	{
		base.LoadReferences(data);
		if (!(data is SaveDataPlayerSettlement saveDataPlayerSettlement))
		{
			return;
		}
		List<Area> list = RuinarchListPool<Area>.Claim();
		GameUtilities.PopulateAreasGivenCoordinates(list, saveDataPlayerSettlement.tileCoordinates, GridMap.Instance.map);
		for (int i = 0; i < list.Count; i++)
		{
			Area p_area = list[i];
			AddAreaToSettlement(p_area);
		}
		RuinarchListPool<Area>.Release(list);
		if (saveDataPlayerSettlement.corruptedTiles != null)
		{
			for (int j = 0; j < saveDataPlayerSettlement.corruptedTiles.Count; j++)
			{
				TileLocationSave tileLocationSave = saveDataPlayerSettlement.corruptedTiles[j];
				if (tileLocationSave.hasValue)
				{
					LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
					corruptedTiles.Add(tileBySavedData);
				}
			}
		}
		SubscribeToListeners();
	}

	public void LoadOtherReferencesUponLoadingSecondWavePlayer()
	{
	}

	public void AddCorruptedTile(LocationGridTile p_tile)
	{
		if (!corruptedTiles.Contains(p_tile))
		{
			corruptedTiles.Add(p_tile);
			Messenger.Broadcast(PlayerSignals.TILE_CORRUPTED, p_tile);
		}
	}

	public void RemoveCorruptedTile(LocationGridTile p_tile)
	{
		if (corruptedTiles.Remove(p_tile))
		{
			Messenger.Broadcast(PlayerSignals.TILE_UNCORRUPTED, p_tile);
		}
	}

	public bool IsCorruptedTileStillConnectedToPortal(LocationGridTile p_tile)
	{
		if (corruptedTiles.Count > 0)
		{
			_ = corruptedTiles[0];
		}
		return false;
	}

	private bool AreTwoCorruptedTilesConnected(LocationGridTile p_startTile, LocationGridTile p_endTile)
	{
		Queue<LocationGridTile> queue = new Queue<LocationGridTile>(100);
		queue.Clear();
		queue.Enqueue(p_startTile);
		while (queue.Count > 0)
		{
			LocationGridTile locationGridTile = queue.Dequeue();
			if (locationGridTile.corruptionComponent.isAlreadyVisitedForIslandCounting)
			{
				continue;
			}
			if (locationGridTile == p_endTile)
			{
				return true;
			}
			locationGridTile.corruptionComponent.SetIsAlreadyVisitedForIslandCounting(p_state: true);
			for (int i = 0; i < locationGridTile.neighbourList.Count; i++)
			{
				LocationGridTile locationGridTile2 = locationGridTile.neighbourList[i];
				if (locationGridTile2 == p_endTile)
				{
					return true;
				}
				if (locationGridTile2.corruptionComponent.isCorrupted)
				{
					queue.Enqueue(locationGridTile2);
				}
			}
		}
		return false;
	}

	public bool AreThereMoreThanOneCorruptedIsland(List<LocationGridTile> p_tileExceptions = null)
	{
		bool result = false;
		if (corruptedTiles.Count > 0 && corruptedTiles[0] != null)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim(300);
			CountLinkedCorruptedTiles(corruptedTiles[0], list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].corruptionComponent.SetIsAlreadyVisitedForIslandCounting(p_state: false);
			}
			if (list.Count < corruptedTiles.Count)
			{
				RuinarchListPool<LocationGridTile>.Release(list);
				return false;
			}
			list.Clear();
			CountLinkedCorruptedTiles(corruptedTiles[0], list, p_tileExceptions);
			int num = list.Count;
			if (p_tileExceptions != null)
			{
				num += p_tileExceptions.Count;
			}
			if (num < corruptedTiles.Count)
			{
				result = true;
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j].corruptionComponent.SetIsAlreadyVisitedForIslandCounting(p_state: false);
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		return result;
	}

	private void CountLinkedCorruptedTiles(LocationGridTile p_startingTile, List<LocationGridTile> p_linkedTiles, List<LocationGridTile> p_tileExceptions = null)
	{
		p_linkedTiles.Add(p_startingTile);
		p_startingTile.corruptionComponent.SetIsAlreadyVisitedForIslandCounting(p_state: true);
		for (int i = 0; i < p_startingTile.neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = p_startingTile.neighbourList[i];
			if (locationGridTile.corruptionComponent.isCorrupted && !locationGridTile.corruptionComponent.isAlreadyVisitedForIslandCounting && (p_tileExceptions == null || !p_tileExceptions.Contains(locationGridTile)))
			{
				CountLinkedCorruptedTiles(locationGridTile, p_linkedTiles, p_tileExceptions);
			}
		}
	}
}
