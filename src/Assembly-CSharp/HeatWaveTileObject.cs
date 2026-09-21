using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;

public class HeatWaveTileObject : AOESpellTileObject
{
	private readonly List<Character> _charactersOutside;

	private string _currentOverheatCheckSchedule;

	private GameObject _effect;

	private string _expiryKey;

	public int expiryInTicks { get; private set; }

	public GameDate expiryDate { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataHeatWaveTileObject);

	protected override int effectRadius => 6;

	public HeatWaveTileObject()
		: base(TILE_OBJECT_TYPE.HEAT_WAVE_TILE_OBJECT)
	{
		_charactersOutside = new List<Character>();
		expiryInTicks = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.HEAT_WAVE);
	}

	public HeatWaveTileObject(SaveDataTileObject data)
		: base(data)
	{
		SaveDataHeatWaveTileObject saveDataHeatWaveTileObject = data as SaveDataHeatWaveTileObject;
		SetExpiryInTicks(saveDataHeatWaveTileObject.expiryInTicks);
		SetIsPlayerSource(saveDataHeatWaveTileObject.isPlayerSource);
		expiryDate = saveDataHeatWaveTileObject.expiryDate;
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
		_expiryKey = SchedulingManager.Instance.AddEntry(expiryDate, delegate
		{
			gridTileLocation.structure.RemovePOI(this);
		}, this);
		PopulateInitialCharactersOutside();
		CreateHeatwaveEffect();
		RescheduleHeatWaveCheck();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
		if (!string.IsNullOrEmpty(_currentOverheatCheckSchedule))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_currentOverheatCheckSchedule);
		}
		ObjectPoolManager.Instance.DestroyObject(_effect);
		ClearCharactersOutside();
	}

	private void CreateHeatwaveEffect()
	{
		GameObject effect = GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Heat_Wave);
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

	private void HeatwaveEffects()
	{
		RESISTANCE resistanceType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.HEAT_WAVE).resistanceType;
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(PLAYER_SKILL_TYPE.HEAT_WAVE);
		int p_value = Mathf.RoundToInt(PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE.HEAT_WAVE));
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		list.AddRange(affectedStructures.Keys);
		for (int i = 0; i < list.Count; i++)
		{
			LocationStructure locationStructure = list[i];
			bool flag = true;
			if (locationStructure is ManMadeStructure manMadeStructure)
			{
				flag = manMadeStructure.CanBeDamagedByPlayerSpells() && (manMadeStructure.wallsAreMadeOf == WALL_RESOURCE.Wood || manMadeStructure.wallsAreMadeOf == WALL_RESOURCE.Elven_Wood || manMadeStructure.wallsAreMadeOf == WALL_RESOURCE.None);
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
				float resistanceValue = character.piercingAndResistancesComponent.GetResistanceValue(resistanceType);
				CombatManager.ModifyValueByPiercingAndResistance(ref p_value, pierceBasedOnCurrentLevel, resistanceValue);
				if (GameUtilities.RollChance(p_value))
				{
					character.traitContainer.AddTrait(character, "Overheating");
					character.traitContainer.GetTraitOrStatus<Overheating>("Overheating")?.SetIsPlayerSource(isPlayerSource);
				}
				else if (character.piercingAndResistancesComponent.GetResistanceValue(resistanceType) > 0f)
				{
					character.reactionComponent.PlayResistVFXandSFX();
				}
			}
		}
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.HEAT_WAVE).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Can_Apply_Burning_To_Random) && GameUtilities.RollChance(50))
		{
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(affectedTiles);
			TileObject tileObject = randomElement.tileObjectComponent.objHere ?? randomElement.tileObjectComponent.genericTileObject;
			tileObject.traitContainer.AddTrait(tileObject, "Burning");
		}
		RescheduleHeatWaveCheck();
	}

	private void RescheduleHeatWaveCheck()
	{
		if (gridTileLocation != null)
		{
			GameDate gameDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(6));
			_currentOverheatCheckSchedule = SchedulingManager.Instance.AddEntry(gameDate, HeatwaveEffects, this);
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
