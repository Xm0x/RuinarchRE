using Inner_Maps;

public class LandmineData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LANDMINE;

	public override string name => "Landmine";

	public override string description => "This Spell places an invisible trap on a target unoccupied tile. Any character that walks into the tile will activate it, causing it to explode and deal Normal damage to a small area around it.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public LandmineData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		targetTile.tileObjectComponent.SetHasLandmine(state: true);
		targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Landmined");
		AkSoundEngine.PostEvent("Play_Place_SFX", InnerMapCameraMove.Instance.gameObject);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			return !targetTile.tileObjectComponent.hasLandmine;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
