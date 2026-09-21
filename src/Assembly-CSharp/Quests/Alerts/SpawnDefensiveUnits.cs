using System;
using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Quests.Alerts;

public class SpawnDefensiveUnits : GameAlert
{
	private int _currentStep;

	private Tilemap _corruptionHighlighterTilemap;

	public override Type serializedData => typeof(SaveDataSpawnDefensiveUnits);

	public int currentStep => _currentStep;

	public SpawnDefensiveUnits()
		: base(Game_Alert.Spawn_Defensive_Units)
	{
		_currentStep = 1;
	}

	public SpawnDefensiveUnits(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Spawn_Defensive_Units)
	{
		SaveDataSpawnDefensiveUnits saveDataSpawnDefensiveUnits = p_data as SaveDataSpawnDefensiveUnits;
		_currentStep = saveDataSpawnDefensiveUnits.currentStep;
	}

	public override void SetAsSpawned()
	{
		TryCreateCorruptionHighlighter();
		Messenger.AddListener<Character>(PlayerSignals.PLAYER_PLACED_DEFENDER, OnPlayerPlacedDefenderForSpawnedAlert);
		Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.ON_FINISH_UNDERLING_COOLDOWN, OnGainMonsterUnderlingCharge);
		Messenger.AddListener<LocationGridTile>(PlayerSignals.TILE_CORRUPTED, OnTileCorrupted);
		Messenger.AddListener<LocationGridTile>(PlayerSignals.TILE_UNCORRUPTED, OnTileUncorrupted);
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		SetActiveStepAs1();
		Messenger.RemoveListener<Character>(PlayerSignals.PLAYER_PLACED_DEFENDER, OnPlayerPlacedDefenderForSpawnedAlert);
		Messenger.RemoveListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.ON_FINISH_UNDERLING_COOLDOWN, OnGainMonsterUnderlingCharge);
	}

	protected override void SetAsCleared()
	{
		base.SetAsCleared();
		if (_corruptionHighlighterTilemap != null)
		{
			UnityEngine.Object.Destroy(_corruptionHighlighterTilemap.gameObject);
		}
		SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
		Messenger.RemoveListener<LocationGridTile>(PlayerSignals.TILE_CORRUPTED, OnTileCorrupted);
		Messenger.RemoveListener<LocationGridTile>(PlayerSignals.TILE_UNCORRUPTED, OnTileUncorrupted);
		Messenger.RemoveListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.ON_FINISH_UNDERLING_COOLDOWN, OnGainMonsterUnderlingCharge);
		Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, OnSetActivePlayerSpellForStep2);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, OnSetActivePlayerSpellForStep3);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpellForStep3);
		Messenger.RemoveListener<Character>(PlayerSignals.PLAYER_PLACED_DEFENDER, OnPlayerPlacedDefenderForStep3);
	}

	public override void LoadReferences(SaveDataGameAlert data)
	{
		base.LoadReferences(data);
		if (_isActive)
		{
			TryCreateCorruptionHighlighter();
			switch (_currentStep)
			{
			case 1:
				SetActiveStepAs1();
				break;
			case 2:
				SetActiveStepAs2();
				break;
			case 3:
				SetActiveStepAs3();
				break;
			}
		}
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		string info = string.Empty;
		if (_currentStep == 1)
		{
			info = GetLocalizedString(_strGameAlertType + "_Tooltip_1");
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Monsters Tab");
		}
		else if (_currentStep == 2)
		{
			info = GetLocalizedString(_strGameAlertType + "_Tooltip_2");
		}
		else if (_currentStep == 3)
		{
			info = GetLocalizedString(_strGameAlertType + "_Tooltip_3");
		}
		UIManager.Instance.ShowSmallInfo(info, p_pos, "", autoReplaceText: false);
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		if (_currentStep == 1)
		{
			Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Monsters Tab");
		}
		UIManager.Instance.HideSmallInfo();
	}

	private void SetActiveStepAs1()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 1;
		_displayName = GetLocalizedString(_strGameAlertType + "_Title") + " (1/3)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.AddListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
		if (PlayerUI.Instance.IsTopMenuToggleOn("Monsters Tab"))
		{
			SetActiveStepAs2();
		}
	}

	private void SetActiveStepAs2()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 2;
		_displayName = GetLocalizedString(_strGameAlertType + "_Title") + " (2/3)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, OnSetActivePlayerSpellForStep2);
	}

	private void SetActiveStepAs3()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 3;
		_displayName = GetLocalizedString(_strGameAlertType + "_Title") + " (3/3)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, OnSetActivePlayerSpellForStep2);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, OnSetActivePlayerSpellForStep3);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpellForStep3);
		Messenger.AddListener<Character>(PlayerSignals.PLAYER_PLACED_DEFENDER, OnPlayerPlacedDefenderForStep3);
		if (PlayerManager.Instance.player.currentActivePlayerSpell is SummonPlayerSkill)
		{
			HighlightCorruption();
			(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal).CenterOnStructure();
		}
	}

	private void OnPlayerPlacedDefenderForSpawnedAlert(Character p_alert)
	{
		SetAsCleared();
	}

	private void OnGainMonsterUnderlingCharge(MonsterAndDemonUnderlingCharges p_underling)
	{
		if (!p_underling.isDemon)
		{
			SetAsActive();
		}
	}

	private void OnToggleClickedForStep1(RuinarchToggle p_toggle)
	{
		if (p_toggle.name == "Monsters Tab" && p_toggle.isOn)
		{
			SetActiveStepAs2();
		}
	}

	private void OnSetActivePlayerSpellForStep2(SkillData p_data)
	{
		if (p_data is SummonPlayerSkill)
		{
			SetActiveStepAs3();
		}
	}

	private void OnSetActivePlayerSpellForStep3(SkillData p_data)
	{
		if (p_data is SummonPlayerSkill)
		{
			HighlightCorruption();
			(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal).CenterOnStructure();
		}
	}

	private void OnPlayerNoActiveSpellForStep3(SkillData p_previousSpell)
	{
		if (p_previousSpell is SummonPlayerSkill)
		{
			UnhighlightCorruption();
		}
	}

	private void OnPlayerPlacedDefenderForStep3(Character p_defender)
	{
		RemoveBookmark();
	}

	private void OnTileCorrupted(LocationGridTile p_tile)
	{
		UnblockTile(p_tile);
	}

	private void OnTileUncorrupted(LocationGridTile p_tile)
	{
		ReblockTile(p_tile);
	}

	private void HighlightCorruption()
	{
		if (_corruptionHighlighterTilemap != null)
		{
			_corruptionHighlighterTilemap.gameObject.SetActive(value: true);
			Color color = _corruptionHighlighterTilemap.color;
			color.a = 0f;
			_corruptionHighlighterTilemap.color = color;
			DOTween.ToAlpha(GetColor, SetColor, 40f / 51f, 0.5f);
		}
	}

	private void SetColor(Color color)
	{
		_corruptionHighlighterTilemap.color = color;
	}

	private Color GetColor()
	{
		return _corruptionHighlighterTilemap.color;
	}

	private void UnhighlightCorruption()
	{
		if (_corruptionHighlighterTilemap != null)
		{
			_corruptionHighlighterTilemap.gameObject.SetActive(value: false);
		}
	}

	private void TryCreateCorruptionHighlighter()
	{
		if (_corruptionHighlighterTilemap == null)
		{
			GameObject gameObject = new GameObject("Corruption Highlighter");
			gameObject.transform.SetParent(GridMap.Instance.mainRegion.innerMap.tilemapsParent);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.layer = LayerMask.NameToLayer("Area Maps");
			_corruptionHighlighterTilemap = gameObject.AddComponent<Tilemap>();
			TilemapRenderer tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();
			tilemapRenderer.sortingLayerName = "Area Maps";
			tilemapRenderer.sortingOrder = 10900;
			_corruptionHighlighterTilemap.BoxFill(new Vector3Int(GridMap.Instance.mainRegion.innerMap.width, GridMap.Instance.mainRegion.innerMap.height, 0), InnerMapManager.Instance.assetManager.minimapTile, 0, 0, GridMap.Instance.mainRegion.innerMap.width, GridMap.Instance.mainRegion.innerMap.height);
			_corruptionHighlighterTilemap.color = new Color(0f, 0f, 0f, 40f / 51f);
			for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.corruptedTiles.Count; i++)
			{
				LocationGridTile p_tile = PlayerManager.Instance.player.playerSettlement.corruptedTiles[i];
				UnblockTile(p_tile);
			}
			_corruptionHighlighterTilemap.gameObject.SetActive(value: false);
		}
	}

	private void UnblockTile(LocationGridTile p_tile)
	{
		if (_corruptionHighlighterTilemap != null)
		{
			_corruptionHighlighterTilemap.SetTile(p_tile.localPlace, null);
		}
	}

	private void ReblockTile(LocationGridTile p_tile)
	{
		if (_corruptionHighlighterTilemap != null)
		{
			_corruptionHighlighterTilemap.SetTile(p_tile.localPlace, InnerMapManager.Instance.assetManager.minimapTile);
		}
	}
}
