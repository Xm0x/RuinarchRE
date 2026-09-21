public class Phylactery : TileObject
{
	public Phylactery()
	{
		Initialize(TILE_OBJECT_TYPE.PHYLACTERY, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		AddAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.AddTrait(this, "Treasure");
	}

	public Phylactery(SaveDataTileObject data)
		: base(data)
	{
	}

	protected override bool IsInterestedInThisTreasure(Character p_character)
	{
		if (!p_character.traitContainer.HasTrait("Vampire"))
		{
			return !p_character.isLycanthrope;
		}
		return false;
	}
}
