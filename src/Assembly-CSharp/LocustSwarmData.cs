using Inner_Maps;

public class LocustSwarmData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LOCUST_SWARM;

	public override string name => "Locust Swarm";

	public override string description => "This Spell spawns a swarm of hungry locusts that would roam around randomly for a few hours, eating everything edible in its path.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 2;

	public LocustSwarmData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		LocustSwarm locustSwarm = new LocustSwarm();
		locustSwarm.SetGridTileLocation(targetTile);
		locustSwarm.OnPlacePOI();
		PlayerManager.Instance.player.playerSkillComponent.SetLatestCastMovingObject(locustSwarm);
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
