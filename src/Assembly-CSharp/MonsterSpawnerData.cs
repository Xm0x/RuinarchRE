using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class MonsterSpawnerData : SkillData
{
	private string _bonusUIText = string.Empty;

	private string _bonusLevelUpUIText = string.Empty;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MONSTER_SPAWNER;

	public override string name => "Monster Spawner";

	public override string description => "Places a Monster Spawner. If inside a landmark, a Monster Spawner will periodically spawn monsters based on the type of landmark it is on.\nIt can also be placed outside of a landmark and it will immediately spawn one random monster. If there is enough empty space around it, the monster will build a new landmark around it.\nIt also spawns Chaos Orbs every 12 hours as long as it has active monsters. Its HP slowly decreases every hour.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public MonsterSpawnerData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		if (targetTile.tileObjectComponent.objHere != null)
		{
			targetTile.structure.RemovePOI(targetTile.tileObjectComponent.objHere);
		}
		MonsterSpawner monsterSpawner = InnerMapManager.Instance.CreateNewTileObject<MonsterSpawner>(TILE_OBJECT_TYPE.MONSTER_SPAWNER);
		monsterSpawner.SetShouldProcessWildernessPlacement(p_state: true);
		targetTile.structure.AddPOI(monsterSpawner, targetTile);
		AkSoundEngine.PostEvent("Play_Spawn_SFX", InnerMapCameraMove.Instance.gameObject);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			if (InnerMapManager.Instance.HasMonsterSpawnerInStructure(targetTile.structure) && targetTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS)
			{
				return false;
			}
			if (targetTile.isOccupied)
			{
				if (targetTile.tileObjectComponent.hiddenObjHere != null)
				{
					return false;
				}
				if (targetTile.tileObjectComponent.objHere != null && !(targetTile.tileObjectComponent.objHere is TreeObject))
				{
					return false;
				}
			}
			if (!targetTile.structure.structureType.IsSpecialStructure() || targetTile.structure.structureType == STRUCTURE_TYPE.MUSHROOM_HAVEN || targetTile.structure.structureType == STRUCTURE_TYPE.BERRY_GARDEN)
			{
				return targetTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS;
			}
			return true;
		}
		return flag;
	}

	public override bool IsValid()
	{
		int monsterSpawnerMaxCapacity = PlayerSkillManager.Instance.monsterSpawnerMaxCapacity;
		if (InnerMapManager.Instance.currentMonsterSpawners.Count >= monsterSpawnerMaxCapacity)
		{
			return false;
		}
		return base.IsValid();
	}

	public override string GetReasonsWhyInvalid()
	{
		string text = base.GetReasonsWhyInvalid();
		int monsterSpawnerMaxCapacity = PlayerSkillManager.Instance.monsterSpawnerMaxCapacity;
		if (InnerMapManager.Instance.currentMonsterSpawners.Count >= monsterSpawnerMaxCapacity)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Monster_Spawner_Capacity_Reached") + "|";
		}
		return text;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		if (tile.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
		{
			TileHighlighter.Instance.PositionHighlight(5, tile);
		}
		else
		{
			TileHighlighter.Instance.HideHighlight();
		}
	}

	public override bool ShowInvalidHighlight(LocationGridTile tile, ref string invalidText)
	{
		if (tile.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
		{
			TileHighlighter.Instance.PositionHighlight(5, tile);
		}
		else
		{
			TileHighlighter.Instance.HideHighlight();
		}
		return true;
	}

	public override string GetBonusUIText()
	{
		if (string.IsNullOrEmpty(_bonusUIText))
		{
			_bonusUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Maximum Spawners") + ":"), PlayerSkillManager.Instance.monsterSpawnerMaxCapacity);
			_bonusUIText = string.Format("{0}\n{1} x{2}", _bonusUIText, Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Max_Spawn_Limit") + ":"), GetSpawnMultiplier());
		}
		return _bonusUIText;
	}

	public override string GetBonusLevelUpUIText()
	{
		if (string.IsNullOrEmpty(_bonusLevelUpUIText))
		{
			int monsterSpawnerMaxCapacity = PlayerSkillManager.Instance.GetMonsterSpawnerMaxCapacity(WorldSettings.Instance.worldSettingsData.mapSettings.mapSize, base.currentLevel + 1);
			if (monsterSpawnerMaxCapacity == 0)
			{
				_bonusLevelUpUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Maximum Spawners") + ":"), PlayerSkillManager.Instance.monsterSpawnerMaxCapacity);
				_bonusLevelUpUIText = string.Format("{0}\n{1} x{2}", _bonusLevelUpUIText, Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Max_Spawn_Limit") + ":"), GetSpawnMultiplier());
			}
			else
			{
				_bonusLevelUpUIText = string.Format("{0} {1} {2} {3}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Maximum Spawners") + ":"), PlayerSkillManager.Instance.monsterSpawnerMaxCapacity, Utilities.UpgradeArrowIcon(), Utilities.ColorizeUpgradeText(monsterSpawnerMaxCapacity.ToString()));
				_bonusLevelUpUIText = string.Format("{0}\n{1} x{2} {3} {4}", _bonusLevelUpUIText, Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Max_Spawn_Limit") + ":"), GetSpawnMultiplier(), Utilities.UpgradeArrowIcon(), Utilities.ColorizeUpgradeText($"x{GetSpawnMultiplier(base.currentLevel + 1)}"));
			}
		}
		return _bonusLevelUpUIText;
	}

	protected override void OnLevelUp()
	{
		base.OnLevelUp();
		ResetBonusUIText();
	}

	private void ResetBonusUIText()
	{
		_bonusUIText = string.Empty;
		_bonusLevelUpUIText = string.Empty;
	}

	public float GetSpawnMultiplier()
	{
		if (base.currentLevel >= 3)
		{
			return 2f;
		}
		if (base.currentLevel >= 1)
		{
			return 1.5f;
		}
		return 1f;
	}

	public float GetSpawnMultiplier(int p_level)
	{
		if (p_level >= 3)
		{
			return 2f;
		}
		if (p_level >= 1)
		{
			return 1.5f;
		}
		return 1f;
	}

	public bool CanPlaceMonsterSpawnerOnTile(LocationGridTile hoveredTile, out string hoverText)
	{
		bool flag = CanPerformAbilityTowards(hoveredTile, out hoverText);
		if (flag && hoveredTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim(121);
			hoveredTile.PopulateTilesInRadius(list, 5, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile tile = list[i];
				if (!CanPlaceStructureOnTile(tile, out var o_cannotPlaceReason))
				{
					hoverText = LocalizationManager.Instance.GetLocalizedValue("PlayerPowerReasons_Table", "Monster_Spawner_No_Space");
					if (!string.IsNullOrEmpty(o_cannotPlaceReason))
					{
						hoverText = hoverText + "\n\t- " + o_cannotPlaceReason;
					}
					hoverText = Utilities.ColorizeInvalidText(hoverText);
					return false;
				}
			}
		}
		return flag;
	}

	private bool CanPlaceStructureOnTile(LocationGridTile tile, out string o_cannotPlaceReason)
	{
		if (tile.structure.structureType != STRUCTURE_TYPE.WILDERNESS)
		{
			o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_not_wilderness");
			return false;
		}
		if (tile.elevationType == ELEVATION.WATER || tile.elevationType == ELEVATION.MOUNTAIN)
		{
			o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_neighbour_not_wilderness");
			return false;
		}
		if (tile.hasBlueprint)
		{
			o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_has_blueprint");
			return false;
		}
		if (tile.IsAtEdgeOfMap())
		{
			o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_edge");
			return false;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		int num = 2;
		tile.PopulateTilesInRadius(list, num, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		List<LocationGridTile> list2 = list;
		for (int i = 0; i < list2.Count; i++)
		{
			LocationGridTile locationGridTile = list2[i];
			if (locationGridTile.hasBlueprint)
			{
				o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_has_blueprint");
				return false;
			}
			if (locationGridTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && locationGridTile.structure.structureType != STRUCTURE_TYPE.CITY_CENTER)
			{
				o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_tiles_away");
				return false;
			}
		}
		if (list != null)
		{
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		o_cannotPlaceReason = string.Empty;
		return true;
	}
}
