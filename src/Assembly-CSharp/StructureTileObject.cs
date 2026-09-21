using Inner_Maps.Location_Structures;

public class StructureTileObject : TileObject
{
	public LocationStructure structureParent => gridTileLocation?.structure;

	public StructureTileObject()
	{
		Initialize(TILE_OBJECT_TYPE.STRUCTURE_TILE_OBJECT, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.BUILD_BLUEPRINT);
		AddAdvertisedAction(INTERACTION_TYPE.TEND_WYVERN_COOP);
		AddAdvertisedAction(INTERACTION_TYPE.DESTROY_HOME);
		AddAdvertisedAction(INTERACTION_TYPE.REPAIR_STRUCTURE);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public StructureTileObject(SaveDataTileObject data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Structure Tile Object " + base.id;
	}

	public override bool CanBeDamaged()
	{
		return false;
	}

	public override bool CanBeSelected()
	{
		return false;
	}

	public override bool OccupiesTile()
	{
		return false;
	}

	public override void SetCharacterOwner(Character characterOwner)
	{
	}
}
