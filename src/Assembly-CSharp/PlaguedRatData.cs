using Inner_Maps;

public class PlaguedRatData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PLAGUED_RAT;

	public override string name => "Plagued Rats";

	public override string description => "This Spell spawns 2 Plagued Rats. These virulent rats will transmit Plague to food sources they come in contact with.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public PlaguedRatData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		targetTile.AddPlaguedRats(p_randomizedPosition: false, isFromSpell: true);
		targetTile.AddPlaguedRats(p_randomizedPosition: true, isFromSpell: true);
		AkSoundEngine.PostEvent("Play_Spawn_SFX", InnerMapCameraMove.Instance.gameObject);
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
