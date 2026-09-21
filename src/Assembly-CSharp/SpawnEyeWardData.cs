using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SpawnEyeWardData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPAWN_EYE_WARD;

	public override string name => "Spawn Eye";

	public override string description => "Spawn a demon eye that will monitor all actions within its radius.";

	public override bool shouldShowOnContextMenu => false;

	public Watcher watcherParentOfEye { get; private set; }

	public SpawnEyeWardData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void Activate(IPlayerActionTarget target, bool bypassChance)
	{
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(this);
	}

	public override void ActivateAbility(LocationGridTile p_targetTile)
	{
		Watcher watcher = watcherParentOfEye;
		if (watcher == null || watcher.hasBeenDestroyed || watcher.eyeWards.Count >= watcher.GetCurrentMaxEyeCount())
		{
			return;
		}
		TileObject hiddenObjHere = p_targetTile.tileObjectComponent.hiddenObjHere;
		if (hiddenObjHere != null)
		{
			p_targetTile.structure.RemovePOI(hiddenObjHere);
		}
		TileObject objHere = p_targetTile.tileObjectComponent.objHere;
		if (objHere != null)
		{
			objHere.AdjustHP(-objHere.maxHP, ELEMENTAL_TYPE.Normal, triggerDeath: true);
			if (objHere.gridTileLocation != null && objHere.gridTileLocation.structure == p_targetTile.structure)
			{
				p_targetTile.structure.RemovePOI(objHere);
			}
		}
		DemonEye demonEye = InnerMapManager.Instance.CreateNewTileObject<DemonEye>(TILE_OBJECT_TYPE.DEMON_EYE);
		demonEye.SetBeholderOwner(watcher);
		p_targetTile.structure.AddPOI(demonEye, p_targetTile);
		watcher.AddEyeWard(demonEye);
		AkSoundEngine.PostEvent("Play_Place_Demonic_Eye", InnerMapCameraMove.Instance.gameObject);
		base.ActivateAbility(p_targetTile);
		Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ACTIVATED, (PlayerAction)this);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Watcher)
		{
			return true;
		}
		return false;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		if (watcherParentOfEye != null)
		{
			TileHighlighter.Instance.PositionHighlight(watcherParentOfEye.GetEyeWardRadius(), tile);
		}
	}

	public override bool CanPerformAbilityTowards(LocationStructure target)
	{
		if (!(target is Watcher watcher))
		{
			return false;
		}
		if (watcher.eyeWards.Count >= watcher.GetCurrentMaxEyeCount())
		{
			return false;
		}
		return true;
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		Watcher watcher = watcherParentOfEye;
		bool flag = true;
		if (watcher == null)
		{
			return false;
		}
		if (flag && watcher.eyeWards.Count >= watcher.GetCurrentMaxEyeCount())
		{
			return false;
		}
		if (flag)
		{
			TileObject objHere = targetTile.tileObjectComponent.objHere;
			if (targetTile.tileObjectComponent.hiddenObjHere != null || (objHere != null && objHere.mapObjectState == MAP_OBJECT_STATE.BUILT && !CanEyeBePlacedOnTopOfTileObject(objHere)) || !targetTile.IsPassable())
			{
				o_cannotPerformReason = LocalizationManager.Instance.GetLocalizedValue("PlayerPowerAlerts_Table", "Spawn Eye Ward invalid_already_has_hidden_object");
				return false;
			}
			return true;
		}
		return flag;
	}

	private bool CanEyeBePlacedOnTopOfTileObject(TileObject tileObject)
	{
		TILE_OBJECT_TYPE tileObjectType = tileObject.tileObjectType;
		if (tileObjectType == TILE_OBJECT_TYPE.SMALL_TREE_OBJECT || (uint)(tileObjectType - 70) <= 3u || tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT)
		{
			return true;
		}
		return false;
	}

	public override void OnSetAsCurrentActiveSpell()
	{
		base.OnSetAsCurrentActiveSpell();
		PlayerManager.Instance.player.tileObjectComponent.ShowAllEyeWardHighlights();
		if (UIManager.Instance.structureInfoUI.isShowing)
		{
			watcherParentOfEye = UIManager.Instance.structureInfoUI.activeStructure as Watcher;
			ActionItem actionItem = UIManager.Instance.structureInfoUI.GetActionItem(this);
			if (actionItem != null)
			{
				actionItem.SetHighlightState(p_state: true);
			}
		}
	}

	public override void OnNoLongerCurrentActiveSpell()
	{
		base.OnNoLongerCurrentActiveSpell();
		PlayerManager.Instance.player.tileObjectComponent.HideAllEyeWardHighlights();
		watcherParentOfEye = null;
		if (UIManager.Instance.structureInfoUI.isShowing)
		{
			ActionItem actionItem = UIManager.Instance.structureInfoUI.GetActionItem(this);
			if (actionItem != null)
			{
				actionItem.SetHighlightState(p_state: false);
			}
		}
	}
}
