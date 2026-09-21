using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using UnityEngine;
using UtilityScripts;

public class DemonicStructurePlayerSkill : SkillData
{
	private LocationStructureObject m_structureTemplate;

	private int _numberOfUncorruptedTiles;

	private string _bonusUIText = string.Empty;

	private string _invalidBuildNotCorrupted;

	private string _notEnoughSpiritEnergyInvalidString;

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE;

	public override string description => name;

	public virtual Vector2Int size => new Vector2Int(0, 0);

	public STRUCTURE_TYPE structureType { get; protected set; }

	private StructureSetting structureSetting => new StructureSetting(structureType, RESOURCE.NONE);

	public override string localizedName => LocalizationManager.Instance.GetLocalizedValue("DemonicStructures_Table", name);

	public override string localizedDescription => LocalizationManager.Instance.GetLocalizedValue("DemonicStructures_Table", name + "_Description") ?? "";

	public LocationStructureObject structureTemplate
	{
		get
		{
			if (m_structureTemplate == null)
			{
				m_structureTemplate = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(FACTION_TYPE.Demons, structureSetting).GetComponent<LocationStructureObject>();
			}
			return m_structureTemplate;
		}
	}

	public DemonicStructurePlayerSkill()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_invalidBuildNotCorrupted = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_not_corrupted");
		_notEnoughSpiritEnergyInvalidString = LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", "Invalid_Not_Enough_Spirit_Energy");
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(p_state: false);
		PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CORRUPT_TILE).ActivateAbility(_numberOfUncorruptedTiles);
		BuildDemonicStructure(targetTile);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		if (base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason))
		{
			if (targetTile.area.structureComponent.CanBuildDemonicStructureHere(structureType, out o_cannotPerformReason) && structureTemplate.HasEnoughSpaceIfPlacedOn(targetTile, out o_cannotPerformReason))
			{
				return CanBuildDemonicStructureOn(structureTemplate, targetTile, out o_cannotPerformReason);
			}
			return false;
		}
		return false;
	}

	public override void OnSetAsCurrentActiveSpell()
	{
		PlayerManager.Instance.ShowStructurePlacementVisual(structureType);
	}

	public override void OnNoLongerCurrentActiveSpell()
	{
		PlayerManager.Instance.HideStructurePlacementVisual();
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		PlayerManager.Instance.SetStructurePlacementVisualHighlightColor(GameUtilities.GetValidTileHighlightColor());
	}

	public override bool ShowInvalidHighlight(LocationGridTile tile, ref string invalidText)
	{
		PlayerManager.Instance.SetStructurePlacementVisualHighlightColor(GameUtilities.GetInvalidTileHighlightColor());
		invalidText = InvalidMessage(tile);
		return true;
	}

	public override string GetBonusUIText()
	{
		if (string.IsNullOrEmpty(_bonusUIText))
		{
			_bonusUIText = string.Format("{0} {1}x{2}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Size") + ":"), size.x, size.y);
		}
		return _bonusUIText;
	}

	public override string GetBonusLevelUpUIText()
	{
		return GetBonusUIText();
	}

	protected virtual string InvalidMessage(LocationGridTile tile)
	{
		return GetCosts();
	}

	private void ResetBonusUIText()
	{
		_bonusUIText = string.Empty;
	}

	private void OnClickNoOnBuildStructureConfirmation()
	{
		PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(p_state: true);
	}

	private void BuildDemonicStructure(LocationGridTile p_tile)
	{
		p_tile.PlaceSelfBuildingStructure(structureSetting, 0);
		Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
	}

	private bool CanBuildDemonicStructureOn(LocationStructureObject structureObj, LocationGridTile centerTile, out string o_cannotPlaceReason)
	{
		InnerTileMap parentMap = centerTile.parentMap;
		_numberOfUncorruptedTiles = 0;
		bool flag = false;
		for (int i = 0; i < structureObj.localOccupiedCoordinates.Count; i++)
		{
			Vector3Int vector3Int = structureObj.localOccupiedCoordinates[i];
			Vector3Int localPlace = centerTile.localPlace;
			int num = vector3Int.x - structureObj.center.x;
			int num2 = vector3Int.y - structureObj.center.y;
			localPlace.x += num;
			localPlace.y += num2;
			if (!Utilities.IsInRange(localPlace.x, 0, parentMap.width) || !Utilities.IsInRange(localPlace.y, 0, parentMap.height))
			{
				continue;
			}
			LocationGridTile locationGridTile = parentMap.map[localPlace.x, localPlace.y];
			if (locationGridTile.corruptionComponent.isCorrupted)
			{
				flag = true;
				continue;
			}
			_numberOfUncorruptedTiles++;
			if (locationGridTile.corruptionComponent.HasCorruptedNeighbour())
			{
				flag = true;
			}
		}
		if (!flag)
		{
			o_cannotPlaceReason = _invalidBuildNotCorrupted;
			return false;
		}
		if (_numberOfUncorruptedTiles > 0)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CORRUPT_TILE);
			bool num3 = skillData.HasValidCharges(_numberOfUncorruptedTiles);
			bool flag2 = skillData.HasEnoughSpiritEnergy(_numberOfUncorruptedTiles);
			if (!num3)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("playerPowerName", skillData.localizedName);
				o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "Invalid_Not_Enough_Charges_Power", dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
				return false;
			}
			if (!flag2)
			{
				o_cannotPlaceReason = _notEnoughSpiritEnergyInvalidString;
				return false;
			}
		}
		o_cannotPlaceReason = string.Empty;
		return true;
	}

	public string GetCosts()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Costs");
		if (WorldSettings.Instance.worldSettingsData.IsSpiritEnergyEnabledBasedOnVictoryCondition())
		{
			return $"{localizedValue}: {Utilities.ManaIcon()}{base.manaCost} {Utilities.SpiritEnergyIcon()}{_numberOfUncorruptedTiles}";
		}
		return $"{localizedValue}: {Utilities.ManaIcon()}{base.manaCost}";
	}
}
