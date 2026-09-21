public abstract class MetalPile : ResourcePile
{
	public MetalPile(TILE_OBJECT_TYPE tileObjectType)
		: base(RESOURCE.METAL)
	{
		Initialize(tileObjectType, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
		SetResourceInPile(100);
	}

	public MetalPile(SaveDataTileObject data)
		: base(data, RESOURCE.METAL)
	{
	}

	public override string ToString()
	{
		return "Metal Pile " + base.id;
	}

	protected override string GetFlavorText()
	{
		return LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "Basic Resource_Flavor_Text");
	}

	public override void GeneralReactionToTileObject(Character actor, ref string debugLog)
	{
		base.GeneralReactionToTileObject(actor, ref debugLog);
		if (actor is Troll && actor.homeStructure != null && gridTileLocation.structure != actor.homeStructure && !actor.jobQueue.HasJob(JOB_TYPE.DROP_ITEM))
		{
			actor.jobComponent.CreateHoardItemJob(this, actor.homeStructure, doNotRecalculate: true);
		}
	}
}
