using Inner_Maps;

public class SpawnNecronomiconData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON;

	public override string name => "Spawn Necronomicon";

	public override string description => "This Spell will create a Necronomicon on the target ground. If an appropriate character picks it up, it will turn into a Necromancer.\nA Necromancer produces a Chaos Orb each time it raises a Skeleton. It also produces 2 Chaos Orbs whenever it or its army of skeletons kill a Villager.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public SpawnNecronomiconData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		targetTile.AddNecronomicon();
		AkSoundEngine.PostEvent("Play_Spawn_SFX", InnerMapCameraMove.Instance.gameObject);
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
