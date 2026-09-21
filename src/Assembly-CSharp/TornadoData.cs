using Inner_Maps;

public class TornadoData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TORNADO;

	public override string name => "Tornado";

	public override string description => "This Spell summons a devastating Tornado that moves around randomly. It deals a high amount of Wind damage to everything it comes in contact with.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 2;

	public TornadoData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		Tornado tornado = new Tornado();
		tornado.SetExpiryDate(GameManager.Instance.Today().AddTicks(PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.TORNADO)));
		tornado.SetGridTileLocation(targetTile);
		tornado.OnPlacePOI();
		tornado.SetIsPlayerSource(p_state: true);
		AkSoundEngine.PostEvent("Play_Tornado_Cast", InnerMapCameraMove.Instance.gameObject);
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
