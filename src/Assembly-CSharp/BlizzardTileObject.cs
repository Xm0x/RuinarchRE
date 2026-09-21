using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class BlizzardTileObject : AOESpellTileObject
{
	private readonly List<Character> _charactersOutside;

	private string _currentFreezingCheckSchedule;

	private GameObject _effect;

	private string _expiryKey;

	public int expiryInTicks { get; private set; }

	public GameDate expiryDate { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataBlizzardTileObject);

	protected override int effectRadius => 6;

	public BlizzardTileObject()
		: base(TILE_OBJECT_TYPE.BLIZZARD_TILE_OBJECT)
	{
		_charactersOutside = new List<Character>();
		expiryInTicks = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BLIZZARD);
	}

	public BlizzardTileObject(SaveDataTileObject data)
		: base(data)
	{
		SaveDataBlizzardTileObject saveDataBlizzardTileObject = data as SaveDataBlizzardTileObject;
		SetExpiryInTicks(saveDataBlizzardTileObject.expiryInTicks);
		SetIsPlayerSource(saveDataBlizzardTileObject.isPlayerSource);
		expiryDate = saveDataBlizzardTileObject.expiryDate;
		_charactersOutside = new List<Character>();
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
		Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
		RescheduleBlizzardDamageAndFreezingProcess();
		if (!expiryDate.hasValue)
		{
			expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
		}
		_expiryKey = SchedulingManager.Instance.AddEntry(expiryDate, delegate
		{
			gridTileLocation.structure.RemovePOI(this);
		}, this);
		PopulateInitialCharactersOutside();
		CreateBlizzardEffect();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
		Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
		if (!string.IsNullOrEmpty(_currentFreezingCheckSchedule))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_currentFreezingCheckSchedule);
		}
		ObjectPoolManager.Instance.DestroyObject(_effect);
		ClearCharactersOutside();
	}

	private void CreateBlizzardEffect()
	{
		GameObject effect = GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Blizzard);
		_effect = effect;
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

	private void OnTickStarted()
	{
		for (int i = 0; i < affectedTiles.Count; i++)
		{
			LocationGridTile locationGridTile = affectedTiles[i];
			if (!locationGridTile.structure.isInterior && GameUtilities.RollChance(25))
			{
				locationGridTile.tileObjectComponent.genericTileObject.traitContainer.RemoveTrait(locationGridTile.tileObjectComponent.genericTileObject, "Burning");
				TileObject objHere = locationGridTile.tileObjectComponent.objHere;
				objHere?.traitContainer.RemoveTrait(objHere, "Burning");
				objHere = locationGridTile.tileObjectComponent.hiddenObjHere;
				objHere?.traitContainer.RemoveTrait(objHere, "Burning");
			}
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

	private void BlizzardDamageAndFreezingProcess()
	{
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.BLIZZARD);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		list.AddRange(affectedStructures.Keys);
		for (int i = 0; i < list.Count; i++)
		{
			LocationStructure locationStructure = list[i];
			bool flag = true;
			if (locationStructure is ManMadeStructure manMadeStructure)
			{
				flag = manMadeStructure.CanBeDamagedByPlayerSpells() && (manMadeStructure.wallsAreMadeOf == WALL_RESOURCE.Stone || manMadeStructure.wallsAreMadeOf == WALL_RESOURCE.Divine_Stone || manMadeStructure.wallsAreMadeOf == WALL_RESOURCE.None);
			}
			else if (locationStructure is NaturalStructure || locationStructure is DemonicStructure)
			{
				flag = false;
			}
			if (flag)
			{
				locationStructure.AdjustHP(-10, null, isPlayerSource: true);
			}
		}
		RuinarchListPool<LocationStructure>.Release(list);
		for (int j = 0; j < _charactersOutside.Count; j++)
		{
			Character character = _charactersOutside[j];
			if (!character.isDead)
			{
				if (!character.traitContainer.HasTrait("Bad Weather"))
				{
					character.traitContainer.AddTrait(character, "Bad Weather");
				}
				character.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Ice, triggerDeath: true, piercingPower: pierceBasedOnCurrentLevel, isPlayerSource: isPlayerSource, source: isPlayerSource ? spellData : null, elementalTraitProcessor: null, showHPBar: true);
				if (isPlayerSource)
				{
					character.OnCharacterHitByPlayerSpell(-damageBaseOnLevel);
				}
				if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
				{
					character.skillCauseOfDeath = PLAYER_SKILL_TYPE.BLIZZARD;
				}
			}
		}
		RescheduleBlizzardDamageAndFreezingProcess();
	}

	private void RescheduleBlizzardDamageAndFreezingProcess()
	{
		if (gridTileLocation != null)
		{
			GameDate gameDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(6));
			_currentFreezingCheckSchedule = SchedulingManager.Instance.AddEntry(gameDate, BlizzardDamageAndFreezingProcess, this);
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
