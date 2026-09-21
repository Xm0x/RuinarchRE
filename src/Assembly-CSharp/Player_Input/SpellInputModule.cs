using Inner_Maps;
using Ruinarch;

namespace Player_Input;

public class SpellInputModule : PlayerInputModule
{
	private LocationGridTile _lastHoveredTile;

	private bool _canTargetLastHoveredTile;

	private bool _hasShownSmallInfo;

	private void InactiveModule()
	{
		InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		PlayerManager.Instance.player.currentActivePlayerSpell.UnhighlightAffectedTiles();
		UIManager.Instance.HideSmallInfo();
		BaseBuildingManager.Instance.DeactivateTileSelectionBoxVisual();
	}

	public override void OnUpdate()
	{
		if (!InnerMapManager.Instance.isAnInnerMapShowing)
		{
			InactiveModule();
			return;
		}
		if (UIManager.Instance.IsMouseOnUI())
		{
			bool flag = false;
			SkillData currentActivePlayerSpell = PlayerManager.Instance.player.currentActivePlayerSpell;
			for (int i = 0; i < currentActivePlayerSpell.targetTypes.Length; i++)
			{
				if (currentActivePlayerSpell.targetTypes[i] == SPELL_TARGET.BASE_BUILDING)
				{
					flag = true;
					break;
				}
			}
			if (flag && BaseBuildingManager.Instance.isPressingLeftClick)
			{
				LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
				IPointOfInterest currentlyHoveredPoi = InnerMapManager.Instance.currentlyHoveredPoi;
				bool flag2 = false;
				string p_hoverText = string.Empty;
				Cursor_Type cursor_Type = Cursor_Type.Default;
				switch (BaseBuildingManager.Instance.BaseBuildingUpdate(currentActivePlayerSpell, tileFromMousePosition, ref p_hoverText))
				{
				case BASE_BUILDING_CODE.Can_Target_By_Left_Click:
					flag2 = true;
					break;
				case BASE_BUILDING_CODE.Cannot_Target_By_Left_Click:
					flag2 = false;
					break;
				}
				if (PlayerManager.Instance.player.currentActivePlayerSpell == null)
				{
					return;
				}
				if (!flag2)
				{
					LocationGridTile locationGridTile = tileFromMousePosition;
					int p_radius = 0;
					if (tileFromMousePosition == null)
					{
						locationGridTile = currentlyHoveredPoi?.gridTileLocation;
					}
					if (currentActivePlayerSpell != null)
					{
						p_radius = currentActivePlayerSpell.radius;
					}
					if (locationGridTile != null && locationGridTile.IsTileConsideredProtected(p_radius))
					{
						p_hoverText = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Protection_Spell_Warning");
					}
				}
				if (cursor_Type != Cursor_Type.None)
				{
					InputManager.Instance.SetCursorTo(cursor_Type);
				}
				else
				{
					InputManager.Instance.SetCursorTo(flag2 ? Cursor_Type.Check : Cursor_Type.Cross);
				}
				if (!string.IsNullOrEmpty(p_hoverText))
				{
					UIManager.Instance.ShowSmallInfo(p_hoverText, "", autoReplaceText: false);
					_hasShownSmallInfo = true;
				}
				else if (_hasShownSmallInfo)
				{
					_hasShownSmallInfo = false;
					UIManager.Instance.HideSmallInfo();
				}
			}
			else
			{
				InactiveModule();
			}
			return;
		}
		LocationGridTile tileFromMousePosition2 = InnerMapManager.Instance.GetTileFromMousePosition();
		bool flag3 = false;
		IPointOfInterest currentlyHoveredPoi2 = InnerMapManager.Instance.currentlyHoveredPoi;
		string hoverText = string.Empty;
		bool autoReplaceText = true;
		SkillData currentActivePlayerSpell2 = PlayerManager.Instance.player.currentActivePlayerSpell;
		Cursor_Type cursor_Type2 = Cursor_Type.None;
		for (int j = 0; j < currentActivePlayerSpell2.targetTypes.Length; j++)
		{
			switch (currentActivePlayerSpell2.targetTypes[j])
			{
			case SPELL_TARGET.CHARACTER:
			case SPELL_TARGET.TILE_OBJECT:
				if (currentlyHoveredPoi2 != null)
				{
					flag3 = currentActivePlayerSpell2.CanTarget(currentlyHoveredPoi2, ref hoverText);
				}
				break;
			case SPELL_TARGET.TILE:
				if (tileFromMousePosition2 != null)
				{
					flag3 = ((!(currentActivePlayerSpell2 is MinionPlayerSkill) && !(currentActivePlayerSpell2 is SummonPlayerSkill)) ? ((!(currentActivePlayerSpell2 is MonsterSpawnerData monsterSpawnerData)) ? currentActivePlayerSpell2.CanTarget(tileFromMousePosition2) : monsterSpawnerData.CanPlaceMonsterSpawnerOnTile(tileFromMousePosition2, out hoverText)) : currentActivePlayerSpell2.CanTarget(tileFromMousePosition2, out hoverText));
					if (currentActivePlayerSpell2 is DemonicStructurePlayerSkill demonicStructurePlayerSkill)
					{
						hoverText = demonicStructurePlayerSkill.GetCosts();
					}
				}
				break;
			case SPELL_TARGET.AREA:
				if (tileFromMousePosition2 != null)
				{
					if (_lastHoveredTile != tileFromMousePosition2)
					{
						_lastHoveredTile = tileFromMousePosition2;
						flag3 = (_canTargetLastHoveredTile = currentActivePlayerSpell2.CanTarget(tileFromMousePosition2.area));
					}
					else
					{
						flag3 = _canTargetLastHoveredTile;
					}
				}
				break;
			case SPELL_TARGET.SETTLEMENT:
			{
				if (tileFromMousePosition2 != null && tileFromMousePosition2.IsPartOfSettlement(out var settlement))
				{
					flag3 = currentActivePlayerSpell2.CanTarget(settlement);
				}
				break;
			}
			case SPELL_TARGET.BASE_BUILDING:
				cursor_Type2 = Cursor_Type.Default;
				switch (BaseBuildingManager.Instance.BaseBuildingUpdate(currentActivePlayerSpell2, tileFromMousePosition2, ref hoverText))
				{
				case BASE_BUILDING_CODE.Can_Target_By_Left_Click:
					flag3 = true;
					break;
				case BASE_BUILDING_CODE.Cannot_Target_By_Left_Click:
					flag3 = false;
					break;
				}
				autoReplaceText = false;
				if (PlayerManager.Instance.player.currentActivePlayerSpell == null)
				{
					return;
				}
				break;
			}
			if (!flag3)
			{
				LocationGridTile locationGridTile2 = tileFromMousePosition2;
				int p_radius2 = 0;
				if (tileFromMousePosition2 == null)
				{
					locationGridTile2 = currentlyHoveredPoi2?.gridTileLocation;
				}
				if (currentActivePlayerSpell2 != null)
				{
					p_radius2 = currentActivePlayerSpell2.radius;
				}
				if (locationGridTile2 != null && locationGridTile2.IsTileConsideredProtected(p_radius2))
				{
					hoverText = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Protection_Spell_Warning");
				}
			}
			if (cursor_Type2 != Cursor_Type.None)
			{
				InputManager.Instance.SetCursorTo(cursor_Type2);
			}
			else
			{
				InputManager.Instance.SetCursorTo(flag3 ? Cursor_Type.Check : Cursor_Type.Cross);
			}
		}
		if (flag3)
		{
			currentActivePlayerSpell2.ShowValidHighlight(tileFromMousePosition2);
		}
		else if (tileFromMousePosition2 == null || !currentActivePlayerSpell2.ShowInvalidHighlight(tileFromMousePosition2, ref hoverText))
		{
			currentActivePlayerSpell2.UnhighlightAffectedTiles();
		}
		if (!string.IsNullOrEmpty(hoverText))
		{
			UIManager.Instance.ShowSmallInfo(hoverText, "", autoReplaceText);
			_hasShownSmallInfo = true;
		}
		else if (_hasShownSmallInfo)
		{
			_hasShownSmallInfo = false;
			UIManager.Instance.HideSmallInfo();
		}
	}
}
