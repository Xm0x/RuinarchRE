using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Kennel : DemonicStructure, CharacterEventDispatcher.IDeathListener
{
	private Summon _occupyingSummon;

	public Summon occupyingSummon => _occupyingSummon;

	public override Type serializedData => typeof(SaveDataKennel);

	public List<LocationGridTile> borderTiles { get; private set; }

	public override SUMMON_TYPE housedMonsterType
	{
		get
		{
			if (occupyingSummon == null)
			{
				return SUMMON_TYPE.None;
			}
			return occupyingSummon.gainedKennelSummonType;
		}
	}

	protected override int maximumConnectedMonsters
	{
		get
		{
			if (occupyingSummon == null)
			{
				return 0;
			}
			return occupyingSummon.gainedKennelSummonCapacity;
		}
	}

	public Kennel(Region location)
		: base(STRUCTURE_TYPE.KENNEL, location)
	{
		SetMaxHPAndReset(2500);
		borderTiles = new List<LocationGridTile>();
	}

	public Kennel(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataKennel saveDataKennel = saveDataLocationStructure as SaveDataKennel;
		if (!string.IsNullOrEmpty(saveDataKennel.occupyingSummonID))
		{
			_occupyingSummon = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataKennel.occupyingSummonID) as Summon;
			_occupyingSummon?.eventDispatcher.SubscribeToCharacterDied(this);
		}
		if (saveDataKennel.borderTiles != null)
		{
			borderTiles = new List<LocationGridTile>();
			for (int i = 0; i < saveDataKennel.borderTiles.Length; i++)
			{
				TileLocationSave tileLocationSave = saveDataKennel.borderTiles[i];
				LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
				borderTiles.Add(tileBySavedData);
			}
		}
	}

	protected override void AfterCharacterAddedToLocation(Character p_character)
	{
		base.AfterCharacterAddedToLocation(p_character);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		Vector3 position = structureObj.transform.position;
		position.x -= 0.5f;
		position.y -= 0.5f;
		worldPosition = position;
	}

	protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false, bool shouldBeCleanedUp = true)
	{
		StopDrainingCharactersHere();
		List<Character> list = RuinarchListPool<Character>.Claim();
		list.AddRange(base.charactersHere);
		for (int i = 0; i < list.Count; i++)
		{
			Character character = list[i];
			character.traitContainer.RemoveRestrainAndImprison(character);
			if (character.isLycanthrope)
			{
				character.lycanData.limboForm.traitContainer.RemoveRestrainAndImprison(character.lycanData.limboForm);
			}
		}
		base.DestroyStructure(p_responsibleCharacter, isPlayerSource, shouldBeCleanedUp: false);
		if (shouldBeCleanedUp)
		{
			MarkForCleanup();
		}
	}

	public bool HasReachedKennelCapacity()
	{
		if (base.preOccupiedBy != null)
		{
			return true;
		}
		return HasValidCharacterInside();
	}

	public override void OnCharacterUnSeizedHere(Character character)
	{
		base.OnCharacterUnSeizedHere(character);
		if (character is Summon summon)
		{
			summon.traitContainer.RestrainAndImprison(summon, null, PlayerManager.Instance.player.playerFaction);
			if (_occupyingSummon == null && IsValidOccupant(summon))
			{
				OccupyKennel(summon);
			}
		}
	}

	public void OnSnatchedCharacterDroppedHere(Character character)
	{
		if (character is Summon summon)
		{
			summon.traitContainer.RestrainAndImprison(summon, null, PlayerManager.Instance.player.playerFaction);
			if (_occupyingSummon == null && IsValidOccupant(summon))
			{
				OccupyKennel(summon);
			}
		}
	}

	protected override void AfterCharacterRemovedFromLocation(Character p_character)
	{
		if (p_character is Summon summon && occupyingSummon == summon)
		{
			UnOccupyKennelAndCheckForNewOccupant();
		}
	}

	public override string GetTestingInfo()
	{
		string text = base.GetTestingInfo();
		if (occupyingSummon != null)
		{
			text = text + "\nOccupying Summon: " + occupyingSummon.name;
		}
		return text + "\nBorder Tiles(" + borderTiles.Count + "): " + borderTiles.ComafyList();
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		List<Character> list = RuinarchListPool<Character>.Claim();
		list.AddRange(base.charactersHere);
		if (base.rooms.FirstOrDefault() is KennelCell kennelCell)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Character character = list[i];
				if (character.faction != null && character.faction.isPlayerFaction)
				{
					LocationGridTile randomPassableTile = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL).GetRandomPassableTile();
					if (randomPassableTile != null)
					{
						character.CancelAllJobs();
						CharacterManager.Instance.Teleport(character, randomPassableTile);
						GameManager.Instance.CreateParticleEffectAt(randomPassableTile, PARTICLE_EFFECT.Minion_Dissipate);
					}
					continue;
				}
				character.traitContainer.RestrainAndImprison(character, null, PlayerManager.Instance.player.playerFaction);
				if (!character.isDead)
				{
					LocationGridTile locationGridTile = kennelCell.tilesInRoom.FirstOrDefault((LocationGridTile t) => t.charactersHere.Count <= 0) ?? CollectionUtilities.GetRandomElement(kennelCell.tilesInRoom);
					if (locationGridTile != null)
					{
						CharacterManager.Instance.Teleport(character, locationGridTile);
						GameManager.Instance.CreateParticleEffectAt(locationGridTile, PARTICLE_EFFECT.Minion_Dissipate);
					}
					if (_occupyingSummon == null && character is Summon summon && IsValidOccupant(summon))
					{
						OccupyKennel(summon);
					}
				}
			}
		}
		RuinarchListPool<Character>.Release(list);
	}

	public override LocationGridTile GetCenterTile()
	{
		return GameUtilities.GetCenterTile(base.tiles, base.tiles.ElementAt(0).parentMap.map);
	}

	public bool HasValidCharacterInside()
	{
		int num = 0;
		for (int i = 0; i < base.charactersHere.Count; i++)
		{
			Character character = base.charactersHere[i];
			if ((!character.movementComponent.isFlying || character.traitContainer.HasTrait("Restrained")) && IsTilePartOfARoom(character.gridTileLocation, out var room) && room.parentStructure == this)
			{
				num++;
			}
		}
		return num > 0;
	}

	public void OccupyKennel(Summon p_summon)
	{
		_occupyingSummon = p_summon;
		occupyingSummon.eventDispatcher.SubscribeToCharacterDied(this);
		PlayerManager.Instance.player.underlingsComponent.GainMonsterUnderlingMaxChargesFromKennel(p_summon.gainedKennelSummonType, p_summon.gainedKennelSummonCapacity);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
		if (_occupyingSummon.summonType == SUMMON_TYPE.Triton)
		{
			PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_CAPTURE_TRITON);
		}
	}

	private void UnoccupyKennel()
	{
		occupyingSummon.eventDispatcher.UnsubscribeToCharacterDied(this);
		PlayerManager.Instance.player.underlingsComponent.LoseMonsterUnderlingMaxChargesFromKennel(occupyingSummon.gainedKennelSummonType, -occupyingSummon.gainedKennelSummonCapacity);
		_occupyingSummon = null;
	}

	private void UnOccupyKennelAndCheckForNewOccupant()
	{
		UnoccupyKennel();
		if (base.charactersHere.FirstOrDefault(IsValidOccupant) is Summon p_summon)
		{
			OccupyKennel(p_summon);
		}
		else
		{
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
		}
	}

	private bool IsValidOccupant(Character p_character)
	{
		if (p_character is Summon summon)
		{
			if (summon.isDead)
			{
				return false;
			}
			if (summon.faction != null && summon.faction.isPlayerFaction)
			{
				return false;
			}
			if (!summon.traitContainer.HasTrait("Restrained"))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private void StopDrainingCharactersHere()
	{
		for (int i = 0; i < base.charactersHere.Count; i++)
		{
			Character character = base.charactersHere[i];
			character.traitContainer.RemoveTrait(character, "Being Drained");
		}
	}

	public void OnCharacterSubscribedToDied(Character p_character)
	{
		UnOccupyKennelAndCheckForNewOccupant();
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
	}

	public void AddBorderTile(LocationGridTile p_tile)
	{
		if (!borderTiles.Contains(p_tile))
		{
			borderTiles.Add(p_tile);
		}
	}

	public void RemoveBorderTile(LocationGridTile p_tile)
	{
		borderTiles.Remove(p_tile);
	}

	public LocationGridTile GetRandomPassableBorderTile()
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < borderTiles.Count; i++)
		{
			LocationGridTile locationGridTile = borderTiles[i];
			if (locationGridTile.IsPassable())
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomBorderTile()
	{
		return CollectionUtilities.GetRandomElement(borderTiles);
	}

	protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom)
	{
		return new KennelCell(tilesInRoom);
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			if (_occupyingSummon != null)
			{
				UnoccupyKennel();
			}
			borderTiles.Clear();
			base.CleanUp();
		}
	}
}
