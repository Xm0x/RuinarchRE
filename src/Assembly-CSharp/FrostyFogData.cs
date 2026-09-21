using Inner_Maps;

public class FrostyFogData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FROSTY_FOG;

	public override string name => "Frosty Fog";

	public override string description => "Frosty Fog";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 1;

	public FrostyFogData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		FrostyFog frostyFog = new FrostyFog();
		frostyFog.SetGridTileLocation(targetTile);
		frostyFog.OnPlacePOI();
		frostyFog.SetStacks(EditableValuesManager.Instance.frostyFogStacks);
		frostyFog.SetIsPlayerSource(p_state: true);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			return targetTile.structure != null;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
