using System.Collections.Generic;
using Inner_Maps;

public class HeatWaveData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HEAT_WAVE;

	public override string name => "Heat Wave";

	public override string description => "This Spell summons a blistering heatwave over a large area. Characters caught outside within the Heatwave may get stacks of Overheating. It does not affect characters inside structures.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 7;

	public HeatWaveData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		List<AOESpellTileObject> activeSpellsOnTile = targetTile.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(targetTile);
		HeatWaveTileObject heatWaveTileObject = null;
		if (activeSpellsOnTile != null)
		{
			for (int i = 0; i < activeSpellsOnTile.Count; i++)
			{
				if (activeSpellsOnTile[i] is HeatWaveTileObject heatWaveTileObject2)
				{
					heatWaveTileObject = heatWaveTileObject2;
					break;
				}
			}
		}
		if (heatWaveTileObject != null)
		{
			heatWaveTileObject.ResetExpiry();
		}
		else
		{
			HeatWaveTileObject heatWaveTileObject3 = InnerMapManager.Instance.CreateNewTileObject<HeatWaveTileObject>(TILE_OBJECT_TYPE.HEAT_WAVE_TILE_OBJECT);
			targetTile.structure.AddPOI(heatWaveTileObject3, targetTile);
			heatWaveTileObject3.SetIsPlayerSource(p_state: true);
		}
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
