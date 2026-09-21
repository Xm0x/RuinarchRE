using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

namespace Player_Input;

public class PickPortalInputModule : PlayerInputModule
{
	private Action _onPortalPlacedAction;

	private LocationStructureObject _portalPrefab;

	private LocationGridTile _lastHoveredTile;

	private bool _canPlacePortalOnCurrentTile;

	private string cannotPerformReason;

	public PickPortalInputModule()
	{
		_portalPrefab = InnerMapManager.Instance.GetStructurePrefabsForStructure(FACTION_TYPE.Demons, STRUCTURE_TYPE.THE_PORTAL, RESOURCE.NONE).First().GetComponent<LocationStructureObject>();
	}

	public void AddOnPortalPlacedAction(Action p_action)
	{
		_onPortalPlacedAction = (Action)Delegate.Combine(_onPortalPlacedAction, p_action);
	}

	public void RemoveOnPortalPlacedAction(Action p_action)
	{
		_onPortalPlacedAction = (Action)Delegate.Remove(_onPortalPlacedAction, p_action);
	}

	public override void OnModuleAdded()
	{
		base.OnModuleAdded();
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	public override void OnModuleRemoved()
	{
		base.OnModuleRemoved();
		Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	public override void OnUpdate()
	{
		LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
		if (tileFromMousePosition != null && !UIManager.Instance.IsMouseOnUI())
		{
			if (_lastHoveredTile == tileFromMousePosition)
			{
				return;
			}
			_lastHoveredTile = tileFromMousePosition;
			Area area = tileFromMousePosition.area;
			string o_cannotPlaceReason;
			bool flag = _portalPrefab.HasEnoughSpaceIfPlacedOn(tileFromMousePosition, out o_cannotPlaceReason);
			string o_cannotBuildReason;
			bool flag2 = area.structureComponent.CanBuildDemonicStructureHere(STRUCTURE_TYPE.THE_PORTAL, out o_cannotBuildReason);
			_canPlacePortalOnCurrentTile = flag && flag2;
			if (!_canPlacePortalOnCurrentTile)
			{
				if (!string.IsNullOrEmpty(o_cannotPlaceReason))
				{
					cannotPerformReason = o_cannotPlaceReason;
				}
				else if (!string.IsNullOrEmpty(o_cannotBuildReason))
				{
					cannotPerformReason = o_cannotBuildReason;
				}
				else
				{
					cannotPerformReason = string.Empty;
				}
			}
			else
			{
				cannotPerformReason = string.Empty;
			}
			Color structurePlacementVisualHighlightColor = (_canPlacePortalOnCurrentTile ? GameUtilities.GetValidTileHighlightColor() : GameUtilities.GetInvalidTileHighlightColor());
			InputManager.Instance.SetCursorTo(_canPlacePortalOnCurrentTile ? Cursor_Type.Check : Cursor_Type.Cross);
			PlayerManager.Instance.SetStructurePlacementVisualHighlightColor(structurePlacementVisualHighlightColor);
		}
		else
		{
			_lastHoveredTile = null;
			InputManager.Instance.SetCursorTo(Cursor_Type.Default);
			TileHighlighter.Instance.HideHighlight();
		}
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Left_Click && !UIManager.Instance.IsMouseOnUI())
		{
			if (_canPlacePortalOnCurrentTile)
			{
				AskForPlacePortalConfirmation(_lastHoveredTile);
			}
			else if (!string.IsNullOrEmpty(cannotPerformReason) && _lastHoveredTile != null)
			{
				InnerMapManager.Instance.ShowAreaMapTextPopup(cannotPerformReason, _lastHoveredTile.centeredWorldLocation, Color.white);
			}
		}
	}

	private void AskForPlacePortalConfirmation(LocationGridTile p_tile)
	{
		PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(p_state: false);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Build_Portal");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Build_Portal_Description");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, delegate
		{
			PlacePortal(p_tile);
		}, OnClickNo, showCover: true, 50, "Yes", "No", yesBtnInteractable: true, noBtnInteractable: true, pauseAndResume: false, yesBtnActive: true, noBtnActive: true, null, null, OnClickClose, OnHideUI);
	}

	private void OnClickNo()
	{
		PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(p_state: true);
	}

	private void OnClickClose()
	{
		PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(p_state: true);
	}

	private void OnHideUI()
	{
		PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(p_state: true);
	}

	private void PlacePortal(LocationGridTile p_tile)
	{
		PlayerSettlement playerSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(p_tile.area);
		playerSettlement.SetName("Demonic Intrusion");
		PlayerManager.Instance.InitializePlayer(p_tile.area, playerSettlement);
		p_tile.InstantPlaceDemonicStructure(new StructureSetting(STRUCTURE_TYPE.THE_PORTAL, RESOURCE.NONE));
		_onPortalPlacedAction?.Invoke();
	}
}
