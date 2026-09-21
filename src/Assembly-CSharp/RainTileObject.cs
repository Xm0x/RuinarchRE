using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;

public class RainTileObject : AOESpellTileObject
{
	private readonly List<Character> _charactersOutside;

	private string _currentRainCheckSchedule;

	private GameObject _effect;

	private string _expiryKey;

	private uint _sfxID;

	public int expiryInTicks { get; private set; }

	public GameDate expiryDate { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataRainTileObject);

	protected override int effectRadius => 6;

	public RainTileObject()
		: base(TILE_OBJECT_TYPE.RAIN_TILE_OBJECT)
	{
		_charactersOutside = new List<Character>();
		expiryInTicks = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.RAIN);
	}

	public RainTileObject(SaveDataTileObject data)
		: base(data)
	{
		SaveDataRainTileObject saveDataRainTileObject = data as SaveDataRainTileObject;
		SetExpiryInTicks(saveDataRainTileObject.expiryInTicks);
		SetIsPlayerSource(saveDataRainTileObject.isPlayerSource);
		expiryDate = saveDataRainTileObject.expiryDate;
		_charactersOutside = new List<Character>();
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
		if (!expiryDate.hasValue)
		{
			expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
		}
		SchedulingManager.Instance.AddEntry(expiryDate, delegate
		{
			gridTileLocation.structure.RemovePOI(this);
		}, this);
		PopulateInitialCharactersOutside();
		CreateRainEffect();
		RescheduleRainCheck();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
		if (!string.IsNullOrEmpty(_currentRainCheckSchedule))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_currentRainCheckSchedule);
		}
		AkSoundEngine.StopPlayingID(_sfxID);
		ObjectPoolManager.Instance.DestroyObject(_effect);
		ClearCharactersOutside();
	}

	private void CreateRainEffect()
	{
		GameObject gameObject = GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Rain);
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Rain", gameObject);
		_effect = gameObject;
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		_charactersOutside?.Remove(p_character);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_charactersOutside.Contains(p_character);
	}

	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure)
	{
		if (structure != null && !structure.isInterior && character.gridTileLocation != null && affectedTiles.Contains(character.gridTileLocation))
		{
			AddCharacterOutside(character);
		}
	}

	private void OnCharacterLeftStructure(Character character, LocationStructure structure)
	{
		if (structure != null && !structure.isInterior && character.gridTileLocation != null && _charactersOutside.Contains(character))
		{
			RemoveCharacterOutside(character);
		}
	}

	private void PopulateInitialCharactersOutside()
	{
		for (int i = 0; i < affectedTiles.Count; i++)
		{
			LocationGridTile locationGridTile = affectedTiles[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if (character.gridTileLocation != null && !character.gridTileLocation.structure.isInterior)
				{
					AddCharacterOutside(character);
				}
			}
		}
	}

	private void ClearCharactersOutside()
	{
		_charactersOutside.Clear();
	}

	private void AddCharacterOutside(Character character)
	{
		if (!_charactersOutside.Contains(character))
		{
			_charactersOutside.Add(character);
		}
	}

	private void RemoveCharacterOutside(Character character)
	{
		_charactersOutside.Remove(character);
	}

	private void CheckForWet()
	{
		for (int i = 0; i < _charactersOutside.Count; i++)
		{
			Character character = _charactersOutside[i];
			if (!character.traitContainer.HasTrait("Bad Weather"))
			{
				character.traitContainer.AddTrait(character, "Bad Weather");
			}
			character.traitContainer.AddTrait(character, "Wet", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Water);
			character.traitContainer.GetTraitOrStatus<Wet>("Wet")?.SetIsPlayerSource(isPlayerSource);
		}
		for (int j = 0; j < affectedTiles.Count; j++)
		{
			LocationGridTile locationGridTile = affectedTiles[j];
			if (!locationGridTile.structure.isInterior)
			{
				MakeTileWet(locationGridTile);
			}
			else if (locationGridTile.structure.structureType == STRUCTURE_TYPE.BARRACKS && locationGridTile.groundType != LocationGridTile.Ground_Type.Wood && locationGridTile.groundType != LocationGridTile.Ground_Type.Structure_Stone)
			{
				MakeTileWet(locationGridTile);
			}
		}
		RescheduleRainCheck();
	}

	private void MakeTileWet(LocationGridTile p_tile)
	{
		p_tile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(p_tile.tileObjectComponent.genericTileObject, "Wet", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Water);
		p_tile.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Wet>("Wet")?.SetIsPlayerSource(isPlayerSource);
		if (p_tile.tileObjectComponent.objHere != null)
		{
			p_tile.tileObjectComponent.objHere.traitContainer.AddTrait(p_tile.tileObjectComponent.objHere, "Wet", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Water);
			if (p_tile.tileObjectComponent.objHere != null)
			{
				p_tile.tileObjectComponent.objHere.traitContainer.GetTraitOrStatus<Wet>("Wet")?.SetIsPlayerSource(isPlayerSource);
			}
		}
	}

	private void RescheduleRainCheck()
	{
		if (gridTileLocation != null)
		{
			GameDate gameDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(15));
			_currentRainCheckSchedule = SchedulingManager.Instance.AddEntry(gameDate, CheckForWet, this);
		}
	}

	private void SetExpiryInTicks(int ticks)
	{
		expiryInTicks = ticks;
	}

	public void ResetExpiry()
	{
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
		}
		expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
		_expiryKey = SchedulingManager.Instance.AddEntry(expiryDate, delegate
		{
			gridTileLocation.structure.RemovePOI(this);
		}, this);
	}

	public override void OnCharacterEnteredTile(Character p_character, LocationGridTile p_tile)
	{
		base.OnCharacterEnteredTile(p_character, p_tile);
		if (p_tile.structure.isInterior)
		{
			RemoveCharacterOutside(p_character);
		}
		else
		{
			AddCharacterOutside(p_character);
		}
	}

	public override void OnCharacterLeftTile(Character p_character, LocationGridTile p_tile)
	{
		base.OnCharacterLeftTile(p_character, p_tile);
		if (p_tile.HasNeighbourNotInList(affectedTiles))
		{
			RemoveCharacterOutside(p_character);
		}
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isPlayerSource = p_state;
	}

	public override string GetAOESpellTestingData()
	{
		return string.Concat(base.GetAOESpellTestingData() + "\n\tCharacters Outside: " + _charactersOutside.ComafyList(), "\n\tExpiry Date: ", expiryDate.ToString());
	}
}
