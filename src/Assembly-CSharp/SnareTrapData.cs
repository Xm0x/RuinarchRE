using Inner_Maps;

public class SnareTrapData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNARE_TRAP;

	public override string name => "Snare Trap";

	public override string description => "This Spell places an invisible trap on a target unoccupied tile. Any character that walks into the tile will activate it and become Ensnared.\nTrapping a hostile Villager produces 2 Chaos Orbs. Additionally, characters Ensnared by the player has a small chance of periodically producing Chaos Orbs.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public SnareTrapData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		targetTile.tileObjectComponent.SetHasSnareTrap(state: true, isPlayerSource: true);
		targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Snare Trapped");
		AkSoundEngine.PostEvent("Play_Place_SFX", InnerMapCameraMove.Instance.gameObject);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		if (base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason))
		{
			return !targetTile.tileObjectComponent.hasSnareTrap;
		}
		return false;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
