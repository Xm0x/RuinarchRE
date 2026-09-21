using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class TortureChambers : DemonicStructure
{
	private TortureChamberStructureObject _tortureChamberStructureObject;

	public List<LocationGridTile> borderTiles { get; private set; }

	public override Type serializedData => typeof(SaveDataTortureChambers);

	public override bool shouldBeLoadedOnMainThread => true;

	public TortureChambers(Region location)
		: base(STRUCTURE_TYPE.TORTURE_CHAMBERS, location)
	{
		SetMaxHPAndReset(2500);
		borderTiles = new List<LocationGridTile>();
	}

	public TortureChambers(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataTortureChambers saveDataTortureChambers = saveDataLocationStructure as SaveDataTortureChambers;
		if (saveDataTortureChambers.borderTiles != null)
		{
			borderTiles = new List<LocationGridTile>();
			for (int i = 0; i < saveDataTortureChambers.borderTiles.Length; i++)
			{
				TileLocationSave tileLocationSave = saveDataTortureChambers.borderTiles[i];
				LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
				borderTiles.Add(tileBySavedData);
			}
		}
	}

	public override void OnCharacterUnSeizedHere(Character character)
	{
		if (character.isNormalCharacter)
		{
			if (character.gridTileLocation != null && IsTilePartOfARoom(character.gridTileLocation, out var room))
			{
				room.GetTileObjectInRoom<DoorTileObject>()?.Close();
			}
			RestrainAndImprisonCharacter(character);
			if (character.partyComponent.hasParty)
			{
				character.partyComponent.currentParty.RemoveMemberThatJoinedQuest(character);
			}
			if (character.gridTileLocation != null && !character.gridTileLocation.charactersHere.Contains(character))
			{
				character.gridTileLocation.AddCharacterHere(character);
			}
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
			if (base.rooms.FirstOrDefault() is PrisonCell prisonCell && prisonCell.IsValidOccupant(character))
			{
				PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_IMPRISON_VILLAGER);
			}
		}
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
		base.DestroyStructure(p_responsibleCharacter, isPlayerSource, shouldBeCleanedUp);
	}

	protected override void AfterCharacterAddedToLocation(Character p_character)
	{
		base.AfterCharacterAddedToLocation(p_character);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
	}

	protected override void AfterCharacterRemovedFromLocation(Character p_character)
	{
		base.AfterCharacterRemovedFromLocation(p_character);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
	}

	public void OnSnatchedCharacterDroppedHere(Character character)
	{
		if (base.rooms.Length != 0 && base.rooms[0] is PrisonCell prisonCell && prisonCell.IsValidOccupant(character))
		{
			RestrainAndImprisonCharacter(character);
			PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_IMPRISON_VILLAGER);
		}
	}

	public override string GetTestingInfo()
	{
		string testingInfo = base.GetTestingInfo();
		return testingInfo + "\nBorder Tiles(" + borderTiles.Count + "): " + borderTiles.ComafyList();
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		if (PlayerManager.Instance.player.playerSettlement.GetNumberOfStructures(STRUCTURE_TYPE.TORTURE_CHAMBERS) >= 1)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BRAINWASH);
			skillData.AdjustMaxCharges(-1);
			skillData.AdjustCharges(-1);
			SkillData skillData2 = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TORTURE);
			skillData2.AdjustMaxCharges(-1);
			skillData2.AdjustCharges(-1);
		}
	}

	protected override string GetCustomDescription()
	{
		return LocalizationManager.Instance.GetLocalizedValue("DemonicStructures_Table", "Torture Chambers_Scenario_Description");
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		_tortureChamberStructureObject = structureObj as TortureChamberStructureObject;
		Vector3 position = structureObj.transform.position;
		position.x -= 0.5f;
		position.y -= 0.5f;
		worldPosition = position;
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		_tortureChamberStructureObject.SetEntrance(base.region.innerMap);
		List<Character> list = RuinarchListPool<Character>.Claim();
		list.AddRange(base.charactersHere);
		if (base.rooms.FirstOrDefault() is PrisonCell prisonCell)
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
				RestrainAndImprisonCharacter(character);
				if (character.isLycanthrope)
				{
					RestrainAndImprisonCharacter(character.lycanData.limboForm);
				}
				LocationGridTile locationGridTile = prisonCell.tilesInRoom.FirstOrDefault((LocationGridTile t) => t.charactersHere.Count <= 0) ?? CollectionUtilities.GetRandomElement(prisonCell.tilesInRoom);
				if (locationGridTile != null)
				{
					CharacterManager.Instance.Teleport(character, locationGridTile);
					GameManager.Instance.CreateParticleEffectAt(locationGridTile, PARTICLE_EFFECT.Minion_Dissipate);
				}
				if (prisonCell.IsValidOccupant(character))
				{
					PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_IMPRISON_VILLAGER);
				}
			}
		}
		RuinarchListPool<Character>.Release(list);
		if (base.settlementLocation.GetNumberOfStructures(STRUCTURE_TYPE.TORTURE_CHAMBERS) > 1)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BRAINWASH);
			skillData.AdjustMaxCharges(1);
			skillData.AdjustCharges(1);
			SkillData skillData2 = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TORTURE);
			skillData2.AdjustMaxCharges(1);
			skillData2.AdjustCharges(1);
		}
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
	}

	public override void OnDoneLoadStructure()
	{
		base.OnDoneLoadStructure();
		_tortureChamberStructureObject.SetEntrance(base.region.innerMap);
	}

	protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom)
	{
		return new PrisonCell(tilesInRoom);
	}

	public bool HasRoomAvailableForSnatchVillager()
	{
		if (base.rooms != null && base.rooms.Length != 0 && base.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell))
		{
			return true;
		}
		return false;
	}

	private bool HasCharacterInPrisonCellThatIsValid(PrisonCell prisonCell)
	{
		return GetFirstCharacterInPrisonCellThatIsValid(prisonCell) != null;
	}

	private Character GetFirstCharacterInPrisonCellThatIsValid(PrisonCell prisonCell)
	{
		for (int i = 0; i < prisonCell.tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = prisonCell.tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if (prisonCell.IsValidOccupant(character) && character.hasMarker)
				{
					return character;
				}
			}
		}
		return null;
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

	private void RestrainAndImprisonCharacter(Character p_character)
	{
		p_character.traitContainer.RestrainAndImprison(p_character, null, PlayerManager.Instance.player.playerFaction);
		if (p_character.faction != null && p_character.faction.isMajorNonPlayer && ChanceData.RollChance(CHANCE_TYPE.Demon_Worship_Crime_On_Imprison))
		{
			p_character.faction.TrySetDemonWorshipAsHeinousCrimeBecauseOfPlayer();
		}
	}

	private void StopDrainingCharactersHere()
	{
		for (int i = 0; i < base.charactersHere.Count; i++)
		{
			Character character = base.charactersHere[i];
			character.traitContainer.RemoveTrait(character, "Being Drained");
		}
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			borderTiles?.Clear();
			_tortureChamberStructureObject = null;
			base.CleanUp();
		}
	}
}
