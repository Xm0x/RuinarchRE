using Inner_Maps;

public class MeteorData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.METEOR;

	public override string name => "Meteor";

	public override string description => "This Spell spawns a flaming Meteor that will crash down and deal major Fire damage to a small target area.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 1;

	public MeteorData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		targetTile.AddMeteor();
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
