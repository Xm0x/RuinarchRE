using System.Collections.Generic;
using Inner_Maps;

public class ElectricStormData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ELECTRIC_STORM;

	public override string name => "Electric Storm";

	public override string description => "This Spell will spawn a series of lightning strikes onto a target area, dealing Electric damage to anything they hit.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 7;

	public ElectricStormData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		List<AOESpellTileObject> activeSpellsOnTile = targetTile.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(targetTile);
		ElectricStormTileObject electricStormTileObject = null;
		if (activeSpellsOnTile != null)
		{
			for (int i = 0; i < activeSpellsOnTile.Count; i++)
			{
				if (activeSpellsOnTile[i] is ElectricStormTileObject electricStormTileObject2)
				{
					electricStormTileObject = electricStormTileObject2;
					break;
				}
			}
		}
		if (electricStormTileObject != null)
		{
			electricStormTileObject.ResetElectricStormDuration();
		}
		else
		{
			ElectricStormTileObject electricStormTileObject3 = InnerMapManager.Instance.CreateNewTileObject<ElectricStormTileObject>(TILE_OBJECT_TYPE.ELECTRIC_STORM_TILE_OBJECT);
			targetTile.structure.AddPOI(electricStormTileObject3, targetTile);
			electricStormTileObject3.SetIsPlayerSource(p_state: true);
		}
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
