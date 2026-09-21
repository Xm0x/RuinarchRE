using System.Collections.Generic;
using Inner_Maps;
using Ruinarch;
using UtilityScripts;

public class SpawnPartyData : PlayerAction
{
	private bool _fromSnatchObject;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPAWN_PARTY;

	public override string name => "Spawn Party";

	public override string description => "Spawn a demonic party for a specific quest.";

	public override bool shouldShowOnContextMenu => false;

	public override int radius => 2;

	public SpawnPartyData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		return false;
	}

	public override void Activate(IPlayerActionTarget target, bool bypassChance)
	{
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(this);
	}

	public void Activate()
	{
		_fromSnatchObject = false;
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(this);
	}

	public void ActivateFromSnatchObject()
	{
		_fromSnatchObject = true;
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(this);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(1, tile);
	}

	public override void ActivateAbility(LocationGridTile p_targetTile)
	{
		if (_fromSnatchObject)
		{
			Messenger.Broadcast(PartySignals.SNATCH_OBJECT_PARTY_TILE_CHOSEN_FOR_SPAWNING, p_targetTile);
		}
		else
		{
			Messenger.Broadcast(PartySignals.PARTY_TILE_CHOSEN_FOR_SPAWNING, p_targetTile);
		}
		base.ActivateAbility(p_targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			targetTile.PopulateTilesInRadius(list, 2);
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile = list[i];
				if (locationGridTile.structure.structureType == STRUCTURE_TYPE.CAVE)
				{
					o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("Party_Table", "invalid_build_at_cave");
					RuinarchListPool<LocationGridTile>.Release(list);
					return false;
				}
				if (locationGridTile.IsWater())
				{
					o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("Party_Table", "invalid_build_at_water");
					RuinarchListPool<LocationGridTile>.Release(list);
					return false;
				}
			}
			list.Clear();
			targetTile.PopulateTilesInRadius(list, 2, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].structure.structureType != STRUCTURE_TYPE.WILDERNESS)
				{
					o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("Party_Table", "invalid_build_at_structure");
					RuinarchListPool<LocationGridTile>.Release(list);
					return false;
				}
			}
			RuinarchListPool<LocationGridTile>.Release(list);
			return true;
		}
		return flag;
	}

	public override void OnNoLongerCurrentActiveSpell()
	{
		base.OnNoLongerCurrentActiveSpell();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		PlayerUI.Instance.EnableTopMenuButtons();
		UIManager.Instance.SetSpeedTogglesState(state: true);
	}

	public override void OnSetAsCurrentActiveSpell()
	{
		base.OnSetAsCurrentActiveSpell();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Pan_Up, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Pan_Down, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Pan_Left, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Pan_Right, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Right_Click, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Toggle_Pause, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Increase_Speed, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Decrease_Speed, p_state: true);
		PlayerUI.Instance.CloseAllTopMenus();
		PlayerUI.Instance.DisableTopMenuButtons();
		UIManager.Instance.SetSpeedTogglesState(state: false);
	}
}
