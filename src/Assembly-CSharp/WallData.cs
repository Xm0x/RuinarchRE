using Inner_Maps;
using Inner_Maps.Location_Structures;

public class WallData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WALL;

	public override string name => "Wall";

	public override string description => "This Spell spawns a single tile of durable wall. Can be chained together to block someone's path.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public WallData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		TileObject objHere = targetTile.tileObjectComponent.objHere;
		if (objHere != null && !objHere.tileObjectType.IsTileObjectImportant())
		{
			targetTile.structure.RemovePOI(objHere);
		}
		BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
		blockWall.SetWallType(WALL_TYPE.Demon_Stone);
		GameDate expiry = GameManager.Instance.Today();
		int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.WALL);
		expiry.AddTicks(durationBonusPerLevel);
		blockWall.SetExpiry(expiry);
		targetTile.structure.AddPOI(blockWall, targetTile);
		AkSoundEngine.PostEvent("Play_Place_SFX", InnerMapCameraMove.Instance.gameObject);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		if (base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason))
		{
			if (targetTile.structure is DemonicStructure)
			{
				return false;
			}
			if (targetTile.charactersHere.Count > 0)
			{
				return false;
			}
			TileObject objHere = targetTile.tileObjectComponent.objHere;
			if (objHere != null)
			{
				if (objHere.tileObjectType.IsTileObjectImportant())
				{
					return false;
				}
				if (objHere.tileObjectType.IsDemonicStructureTileObject())
				{
					return false;
				}
				if (objHere.tileObjectType == TILE_OBJECT_TYPE.DOOR_TILE_OBJECT)
				{
					return false;
				}
				if (objHere.traitContainer.HasTrait("Indestructible"))
				{
					return false;
				}
			}
			if (targetTile.structure != null)
			{
				return targetTile.structure.structureType != STRUCTURE_TYPE.OCEAN;
			}
			return false;
		}
		return false;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
