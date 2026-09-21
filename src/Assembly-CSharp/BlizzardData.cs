using System.Collections.Generic;
using Inner_Maps;

public class BlizzardData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BLIZZARD;

	public override string name => "Blizzard";

	public override string description => "This Spell summons a chilling Blizzard over a large area. Characters caught outside within the Blizzard may get stacks of Freezing, eventually causing them to be Frozen in place. It does not affect characters inside structures and caves.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 7;

	public BlizzardData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		List<AOESpellTileObject> activeSpellsOnTile = targetTile.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(targetTile);
		BlizzardTileObject blizzardTileObject = null;
		if (activeSpellsOnTile != null)
		{
			for (int i = 0; i < activeSpellsOnTile.Count; i++)
			{
				if (activeSpellsOnTile[i] is BlizzardTileObject blizzardTileObject2)
				{
					blizzardTileObject = blizzardTileObject2;
					break;
				}
			}
		}
		if (blizzardTileObject != null)
		{
			blizzardTileObject.ResetExpiry();
		}
		else
		{
			BlizzardTileObject blizzardTileObject3 = InnerMapManager.Instance.CreateNewTileObject<BlizzardTileObject>(TILE_OBJECT_TYPE.BLIZZARD_TILE_OBJECT);
			targetTile.structure.AddPOI(blizzardTileObject3, targetTile);
			blizzardTileObject3.SetIsPlayerSource(p_state: true);
		}
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
