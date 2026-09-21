using System.Collections.Generic;
using Inner_Maps;

public class RainData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAIN;

	public override string name => "Rain";

	public override string description => "This Spell will generate rainfall on the target area, applying Wet to anything outside structures and caves.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 7;

	public RainData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		List<AOESpellTileObject> activeSpellsOnTile = targetTile.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(targetTile);
		RainTileObject rainTileObject = null;
		if (activeSpellsOnTile != null)
		{
			for (int i = 0; i < activeSpellsOnTile.Count; i++)
			{
				if (activeSpellsOnTile[i] is RainTileObject rainTileObject2)
				{
					rainTileObject = rainTileObject2;
					break;
				}
			}
		}
		if (rainTileObject != null)
		{
			rainTileObject.ResetExpiry();
		}
		else
		{
			RainTileObject rainTileObject3 = InnerMapManager.Instance.CreateNewTileObject<RainTileObject>(TILE_OBJECT_TYPE.RAIN_TILE_OBJECT);
			targetTile.structure.AddPOI(rainTileObject3, targetTile);
			rainTileObject3.SetIsPlayerSource(p_state: true);
		}
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
