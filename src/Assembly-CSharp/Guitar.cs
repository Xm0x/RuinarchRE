using Characters.Villager_Wants;

public class Guitar : TileObject
{
	public Guitar()
	{
		Initialize(TILE_OBJECT_TYPE.GUITAR);
		AddAdvertisedAction(INTERACTION_TYPE.PLAY_GUITAR);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
	}

	public Guitar(SaveDataTileObject data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Guitar " + base.id;
	}

	protected override void OnSetObjectAsUnbuilt()
	{
		base.OnSetObjectAsUnbuilt();
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
	}

	protected override void OnSetObjectAsBuilt()
	{
		base.OnSetObjectAsBuilt();
		RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
		RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (!actor.partyComponent.isMemberThatJoinedQuest)
		{
			TryCreateObtainFurnitureWantOnReactionJob<GuitarWant>(actor);
		}
	}
}
