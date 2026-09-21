using Inner_Maps.Location_Structures;

public class Heirloom : TileObject
{
	public override string description => LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "NoValue_Flavor_Text");

	public LocationStructure structureSpot { get; private set; }

	public Heirloom()
	{
		Initialize(TILE_OBJECT_TYPE.HEIRLOOM);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public Heirloom(SaveDataTileObject data)
		: base(data)
	{
	}

	public void SetStructureSpot(LocationStructure structure)
	{
		structureSpot = structure;
	}

	public bool IsInStructureSpot()
	{
		if (gridTileLocation != null)
		{
			return gridTileLocation.structure == structureSpot;
		}
		return false;
	}
}
