using Inner_Maps;

public class PoisonCloudData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.POISON_CLOUD;

	public override string name => "Poison Cloud";

	public override string description => "This Spell spawns a small Poison Cloud. A Poison Cloud may apply Poison to objects and characters it touches.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 1;

	public PoisonCloudData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		PoisonCloud poisonCloud = InnerMapManager.Instance.SpawnPoisonCloud(targetTile, 5, GameManager.Instance.Today().AddTicks(PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.POISON_CLOUD)));
		poisonCloud.SetIsPlayerSource(p_state: true);
		PlayerManager.Instance.player.playerSkillComponent.SetLatestCastMovingObject(poisonCloud);
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
