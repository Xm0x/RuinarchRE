using Inner_Maps;

public class SpawnRatmanData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPAWN_RATMAN;

	public override string name => "Spawn Ratman";

	public override string description => "This Spell will spawns a single Ratman on the target ground.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public SpawnRatmanData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		Character character = CharacterManager.Instance.GenerateRatman(targetTile);
		if (targetTile.structure != null && CharacterManager.Instance.ShouldMonsterRelocateHomeToStructureOnPlace(targetTile.structure, character))
		{
			character.MigrateHomeStructureTo(targetTile.structure);
		}
		AkSoundEngine.PostEvent("Play_Spawn_SFX", InnerMapCameraMove.Instance.gameObject);
		base.ActivateAbility(targetTile);
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
