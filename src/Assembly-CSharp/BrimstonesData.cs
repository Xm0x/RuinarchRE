using System.Collections.Generic;
using Inner_Maps;

public class BrimstonesData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BRIMSTONES;

	public override string name => "Brimstones";

	public override string description => "This Spell will make dozens of burning rocks come crashing down from space onto a target area, dealing Fire damage to anything they hit.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 7;

	public BrimstonesData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		List<AOESpellTileObject> activeSpellsOnTile = targetTile.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(targetTile);
		BrimstonesTileObject brimstonesTileObject = null;
		if (activeSpellsOnTile != null)
		{
			for (int i = 0; i < activeSpellsOnTile.Count; i++)
			{
				if (activeSpellsOnTile[i] is BrimstonesTileObject brimstonesTileObject2)
				{
					brimstonesTileObject = brimstonesTileObject2;
					break;
				}
			}
		}
		if (brimstonesTileObject != null)
		{
			brimstonesTileObject.ResetDuration();
		}
		else
		{
			BrimstonesTileObject brimstonesTileObject3 = InnerMapManager.Instance.CreateNewTileObject<BrimstonesTileObject>(TILE_OBJECT_TYPE.BRIMSTONES_TILE_OBJECT);
			targetTile.structure.AddPOI(brimstonesTileObject3, targetTile);
			brimstonesTileObject3.SetIsPlayerSource(p_state: true);
		}
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
