using System.Collections.Generic;
using Inner_Maps;

public class EarthquakeData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EARTHQUAKE;

	public override string name => "Earthquake";

	public override string description => "This Spell will cause the ground to shake vigorously, dealing a small amount of Earth damage to everyone in range. Objects may get moved around.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 7;

	public EarthquakeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		List<AOESpellTileObject> activeSpellsOnTile = targetTile.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(targetTile);
		EarthquakeTileObject earthquakeTileObject = null;
		if (activeSpellsOnTile != null)
		{
			for (int i = 0; i < activeSpellsOnTile.Count; i++)
			{
				if (activeSpellsOnTile[i] is EarthquakeTileObject earthquakeTileObject2)
				{
					earthquakeTileObject = earthquakeTileObject2;
					break;
				}
			}
		}
		if (earthquakeTileObject != null)
		{
			earthquakeTileObject.ResetDuration();
		}
		else
		{
			EarthquakeTileObject poi = InnerMapManager.Instance.CreateNewTileObject<EarthquakeTileObject>(TILE_OBJECT_TYPE.EARTHQUAKE_TILE_OBJECT);
			targetTile.structure.AddPOI(poi, targetTile);
		}
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
