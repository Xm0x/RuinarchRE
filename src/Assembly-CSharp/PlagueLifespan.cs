using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class PlagueLifespan
{
	private int _tileObjectInfectionTimeInHours;

	private int _monsterInfectionTimeInHours;

	private int _undeadInfectionTimeInHours;

	private Dictionary<RACE, int> _sapientInfectionTimeInHours;

	public const int MAX_LEVEL = 4;

	public int tileObjectInfectionTimeInHours => _tileObjectInfectionTimeInHours;

	public int monsterInfectionTimeInHours => _monsterInfectionTimeInHours;

	public int undeadInfectionTimeInHours => _undeadInfectionTimeInHours;

	public Dictionary<RACE, int> sapientInfectionTimeInHours => _sapientInfectionTimeInHours;

	public PlagueLifespan()
	{
		Initialize();
	}

	public PlagueLifespan(SaveDataPlagueLifespan p_data)
	{
		_tileObjectInfectionTimeInHours = p_data.tileObjectInfectionTimeInHours;
		_monsterInfectionTimeInHours = p_data.monsterInfectionTimeInHours;
		_undeadInfectionTimeInHours = p_data.undeadInfectionTimeInHours;
		_sapientInfectionTimeInHours = p_data.sapientInfectionTimeInHours;
	}

	private void Initialize()
	{
		_sapientInfectionTimeInHours = new Dictionary<RACE, int>();
		SetTileObjectInfectionTimeInHours(24);
		SetMonsterInfectionTimeInHours(-1);
		SetUndeadInfectionTimeInHours(-1);
		SetSapientInfectionTimeInHours(RACE.HUMANS, 48);
		SetSapientInfectionTimeInHours(RACE.ELVES, 48);
	}

	public void SetTileObjectInfectionTimeInHours(int p_hours)
	{
		_tileObjectInfectionTimeInHours = p_hours;
	}

	public void UpgradeTileObjectInfectionTime()
	{
		SetTileObjectInfectionTimeInHours(GetUpgradedTileObjectInfectionTime());
	}

	public int GetUpgradedTileObjectInfectionTime()
	{
		switch (GetTileObjectLifespanLevelByHours(_tileObjectInfectionTimeInHours))
		{
		case 1:
			return GetTileObjectLifespanInHoursByLevel(2);
		case 2:
			return GetTileObjectLifespanInHoursByLevel(3);
		case 3:
			return GetTileObjectLifespanInHoursByLevel(4);
		default:
			Debug.LogError("Could not upgrade Tile Object Infection Time: " + _tileObjectInfectionTimeInHours);
			return -1;
		}
	}

	public int GetTileObjectInfectionTimeUpgradeCost()
	{
		return GetTileObjectLifespanLevelByHours(_tileObjectInfectionTimeInHours) switch
		{
			1 => SpellUtilities.GetModifiedSpellCost(10, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			2 => SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			3 => SpellUtilities.GetModifiedSpellCost(50, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			_ => -1, 
		};
	}

	public bool IsTileObjectAtMaxLevel()
	{
		return _tileObjectInfectionTimeInHours == GetTileObjectLifespanInHoursByLevel(4);
	}

	private int GetTileObjectLifespanInHoursByLevel(int p_level)
	{
		return p_level switch
		{
			1 => 24, 
			2 => 48, 
			3 => 72, 
			4 => 96, 
			_ => 24, 
		};
	}

	private int GetTileObjectLifespanLevelByHours(int p_hours)
	{
		return p_hours switch
		{
			24 => 1, 
			48 => 2, 
			72 => 3, 
			96 => 4, 
			_ => 1, 
		};
	}

	public int GetTileObjectLifespanCurrentLevel()
	{
		return GetTileObjectLifespanLevelByHours(_tileObjectInfectionTimeInHours);
	}

	public void SetMonsterInfectionTimeInHours(int p_hours)
	{
		_monsterInfectionTimeInHours = p_hours;
	}

	public void UpgradeMonsterInfectionTime()
	{
		SetMonsterInfectionTimeInHours(GetUpgradedMonsterInfectionTime());
	}

	public int GetUpgradedMonsterInfectionTime()
	{
		switch (GetMonsterLifespanLevelByHours(_monsterInfectionTimeInHours))
		{
		case 1:
			return GetMonsterLifespanInHoursByLevel(2);
		case 2:
			return GetMonsterLifespanInHoursByLevel(3);
		case 3:
			return GetMonsterLifespanInHoursByLevel(4);
		default:
			Debug.LogError("Could not upgrade Monster Infection Time: " + _monsterInfectionTimeInHours);
			return -1;
		}
	}

	public int GetMonsterInfectionTimeUpgradeCost()
	{
		return GetMonsterLifespanLevelByHours(_monsterInfectionTimeInHours) switch
		{
			1 => SpellUtilities.GetModifiedSpellCost(10, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			2 => SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			3 => SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			_ => -1, 
		};
	}

	public bool IsMonstersAtMaxLevel()
	{
		return _monsterInfectionTimeInHours == GetMonsterLifespanInHoursByLevel(4);
	}

	private int GetMonsterLifespanInHoursByLevel(int p_level)
	{
		return p_level switch
		{
			1 => -1, 
			2 => 24, 
			3 => 72, 
			4 => 120, 
			_ => -1, 
		};
	}

	private int GetMonsterLifespanLevelByHours(int p_hours)
	{
		return p_hours switch
		{
			-1 => 1, 
			24 => 2, 
			72 => 3, 
			120 => 4, 
			_ => 1, 
		};
	}

	public int GetMonstersLifespanCurrentLevel()
	{
		return GetMonsterLifespanLevelByHours(_monsterInfectionTimeInHours);
	}

	public void SetUndeadInfectionTimeInHours(int p_hours)
	{
		_undeadInfectionTimeInHours = p_hours;
	}

	public void UpgradeUndeadInfectionTime()
	{
		SetUndeadInfectionTimeInHours(GetUpgradedUndeadInfectionTime());
	}

	public int GetUpgradedUndeadInfectionTime()
	{
		switch (GetUndeadLifespanLevelByHours(_undeadInfectionTimeInHours))
		{
		case 1:
			return GetUndeadLifespanInHoursByLevel(2);
		case 2:
			return GetUndeadLifespanInHoursByLevel(3);
		case 3:
			return GetUndeadLifespanInHoursByLevel(4);
		default:
			Debug.LogError("Could not upgrade Undead Infection Time: " + _undeadInfectionTimeInHours);
			return -1;
		}
	}

	public int GetUndeadInfectionTimeUpgradeCost()
	{
		return GetUndeadLifespanLevelByHours(_undeadInfectionTimeInHours) switch
		{
			1 => SpellUtilities.GetModifiedSpellCost(10, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			2 => SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			3 => SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			_ => -1, 
		};
	}

	public bool IsUndeadAtMaxLevel()
	{
		return _undeadInfectionTimeInHours == GetUndeadLifespanInHoursByLevel(4);
	}

	private int GetUndeadLifespanInHoursByLevel(int p_level)
	{
		return p_level switch
		{
			1 => -1, 
			2 => 24, 
			3 => 72, 
			4 => 120, 
			_ => -1, 
		};
	}

	private int GetUndeadLifespanLevelByHours(int p_hours)
	{
		return p_hours switch
		{
			-1 => 1, 
			24 => 2, 
			72 => 3, 
			120 => 4, 
			_ => 1, 
		};
	}

	public int GetUndeadLifespanCurrentLevel()
	{
		return GetUndeadLifespanLevelByHours(_undeadInfectionTimeInHours);
	}

	public void SetSapientInfectionTimeInHours(RACE p_race, int p_hours)
	{
		if (_sapientInfectionTimeInHours.ContainsKey(p_race))
		{
			_sapientInfectionTimeInHours[p_race] = p_hours;
		}
		else
		{
			_sapientInfectionTimeInHours.Add(p_race, p_hours);
		}
	}

	public void UpgradeSapientInfectionTime(RACE p_race)
	{
		SetSapientInfectionTimeInHours(p_race, GetUpgradedSapientInfectionTime(p_race));
	}

	public int GetUpgradedSapientInfectionTime(RACE p_race)
	{
		int sapientLifespanOfPlagueInHours = GetSapientLifespanOfPlagueInHours(p_race);
		switch (GetSapientLifespanLevelByHours(p_race, sapientLifespanOfPlagueInHours))
		{
		case 1:
			return GetSapientLifespanInHoursByLevel(p_race, 2);
		case 2:
			return GetSapientLifespanInHoursByLevel(p_race, 3);
		case 3:
			return GetSapientLifespanInHoursByLevel(p_race, 4);
		default:
			Debug.LogError("Could not upgrade " + p_race.ToString() + " Sapient Infection Time: " + sapientLifespanOfPlagueInHours);
			return -1;
		}
	}

	public int GetSapientInfectionTimeUpgradeCost(RACE p_race)
	{
		int sapientLifespanOfPlagueInHours = GetSapientLifespanOfPlagueInHours(p_race);
		return GetSapientLifespanLevelByHours(p_race, sapientLifespanOfPlagueInHours) switch
		{
			1 => SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			2 => SpellUtilities.GetModifiedSpellCost(40, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			3 => SpellUtilities.GetModifiedSpellCost(60, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			_ => -1, 
		};
	}

	public int GetSapientLifespanOfPlagueInHours(RACE p_race)
	{
		if (_sapientInfectionTimeInHours.ContainsKey(p_race))
		{
			return _sapientInfectionTimeInHours[p_race];
		}
		return -1;
	}

	public bool IsSapientAtMaxLevel(RACE p_race)
	{
		return GetSapientLifespanOfPlagueInHours(p_race) == GetSapientLifespanInHoursByLevel(p_race, 4);
	}

	private int GetSapientLifespanInHoursByLevel(RACE p_race, int p_level)
	{
		return p_level switch
		{
			1 => 48, 
			2 => 96, 
			3 => 144, 
			4 => 192, 
			_ => 48, 
		};
	}

	private int GetSapientLifespanLevelByHours(RACE p_race, int p_hours)
	{
		return p_hours switch
		{
			48 => 1, 
			96 => 2, 
			144 => 3, 
			192 => 4, 
			_ => 1, 
		};
	}

	public int GetSapientLifespanCurrentLevel(RACE p_race)
	{
		return GetSapientLifespanLevelByHours(p_race, sapientInfectionTimeInHours[p_race]);
	}

	private int GetLifespanInHoursOfPlagueOn(IPointOfInterest p_poi)
	{
		if (p_poi is Character character)
		{
			Faction faction = character.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Wild_Monsters)
			{
				return monsterInfectionTimeInHours;
			}
			Faction faction2 = character.faction;
			if (faction2 != null && faction2.factionType.type == FACTION_TYPE.Undead)
			{
				return undeadInfectionTimeInHours;
			}
			if (character is Summon summon)
			{
				if (summon.IsUndead())
				{
					return undeadInfectionTimeInHours;
				}
				return monsterInfectionTimeInHours;
			}
			return GetSapientLifespanOfPlagueInHours(character.race);
		}
		if (p_poi is TileObject)
		{
			return tileObjectInfectionTimeInHours;
		}
		return -1;
	}

	public int GetLifespanInTicksOfPlagueOn(IPointOfInterest p_poi)
	{
		int lifespanInHoursOfPlagueOn = GetLifespanInHoursOfPlagueOn(p_poi);
		if (lifespanInHoursOfPlagueOn != -1)
		{
			return GameManager.Instance.GetTicksBasedOnHour(lifespanInHoursOfPlagueOn);
		}
		return -1;
	}

	public string GetInfectionTimeString(int timeInHours)
	{
		return timeInHours switch
		{
			0 => LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Indefinite"), 
			-1 => LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Immune"), 
			_ => timeInHours + " " + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Hours"), 
		};
	}
}
