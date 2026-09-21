using Inner_Maps.Location_Structures;

public class WaterWell : TileObject
{
	public WaterWell()
	{
		Initialize(TILE_OBJECT_TYPE.WATER_WELL);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.traitContainer.AddTrait(this, "Immovable");
		base.traitContainer.AddTrait(this, "Frozen Immune");
		for (int i = 0; i < 10; i++)
		{
			base.traitContainer.AddTrait(this, "Wet", null, bypassElementalChance: true, 0, 0f, ELEMENTAL_TYPE.Water);
		}
	}

	public WaterWell(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (base.structureLocation.structureType != STRUCTURE_TYPE.OCEAN)
		{
			AddAdvertisedAction(INTERACTION_TYPE.WELL_JUMP);
			AddAdvertisedAction(INTERACTION_TYPE.REPAIR);
			AddAdvertisedAction(INTERACTION_TYPE.DRINK_WATER);
		}
		Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener(Signals.HOUR_STARTED, HourStarted);
	}

	public override bool CanBeAffectedByElementalStatus(string traitName)
	{
		if (traitName == "Wet")
		{
			return true;
		}
		return base.structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
	}

	public override bool CanBeDamaged()
	{
		return base.structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
	}

	public override bool CanBeSelected()
	{
		if (base.structureLocation != null)
		{
			return base.structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
		}
		return false;
	}

	public override string ToString()
	{
		return "Well " + base.id;
	}

	private void HourStarted()
	{
		if (base.traitContainer.stacks.ContainsKey("Wet") && base.traitContainer.stacks["Wet"] < 10)
		{
			base.traitContainer.AddTrait(this, "Wet", null, bypassElementalChance: true, 0, 0f, ELEMENTAL_TYPE.Water);
		}
	}

	protected override void OnSetObjectAsUnbuilt()
	{
		if (base.structureLocation is CityCenter)
		{
			mapVisual.SetVisualAlpha(0f);
			SetSlotAlpha(0f);
			SetPOIState(POI_STATE.INACTIVE);
			AddAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
			UnsubscribeListeners();
			base.constructionComponent.OnObjectSetAsUnbuilt();
		}
		else
		{
			base.OnSetObjectAsUnbuilt();
		}
	}
}
