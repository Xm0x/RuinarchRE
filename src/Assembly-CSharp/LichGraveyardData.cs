using Inner_Maps;
using Inner_Maps.Location_Structures;

public class LichGraveyardData : SkillData
{
	private LocationStructureObject _structureTemplate;

	private StructureSetting _structureSetting;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LICH_GRAVEYARD;

	public override string name => "Lich Graveyard";

	public override string description => "This Spell will spawns a Lich Graveyard that will regularly generate Undead creatures.\nSpawned Undead will only attack nearby Villages.\nThere can only be one active Lich Graveyard at a time.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 2;

	private STRUCTURE_TYPE _structureType => STRUCTURE_TYPE.LICH_GRAVEYARD;

	private LocationStructureObject structureTemplate
	{
		get
		{
			if (_structureTemplate == null)
			{
				_structureTemplate = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(FACTION_TYPE.Demons, _structureSetting).GetComponent<LocationStructureObject>();
			}
			return _structureTemplate;
		}
	}

	public LichGraveyardData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_structureSetting = new StructureSetting(_structureType, RESOURCE.NONE);
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		Area area = targetTile.area;
		NPCSettlement p_settlement = LandmarkManager.Instance.CreateNewSettlement(area.region, LOCATION_TYPE.DUNGEON, area);
		targetTile.tileObjectComponent.genericTileObject.InstantPlaceStructure(structureTemplate.gameObject.name, p_settlement);
		LichGraveyard lichGraveyard = targetTile.structure as LichGraveyard;
		lichGraveyard.SetSpawnRate(GameManager.Instance.GetTicksBasedOnHour(2));
		lichGraveyard.SetMaxCapacity(5);
		lichGraveyard.SetFaction(FactionManager.Instance.undeadFaction);
		lichGraveyard.StartSpawning();
		if (lichGraveyard.structureObj != null)
		{
			AkSoundEngine.PostEvent("Play_Place_Demonic_Structure", lichGraveyard.structureObj.gameObject);
		}
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		if (base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason))
		{
			if (targetTile.area.structureComponent.CanBuildNormalStructureHere(_structureType, out o_cannotPerformReason))
			{
				return structureTemplate.HasEnoughSpaceIfPlacedOn(targetTile, out o_cannotPerformReason);
			}
			return false;
		}
		return false;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
