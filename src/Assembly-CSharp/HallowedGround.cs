using UnityEngine;

public class HallowedGround : TileObject
{
	public override Vector2 selectableSize => new Vector2(3f, 3f);

	public HallowedGround()
	{
		Initialize(TILE_OBJECT_TYPE.HALLOWED_GROUND);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		AddAdvertisedAction(INTERACTION_TYPE.PILGRIMAGE);
		AddAdvertisedAction(INTERACTION_TYPE.CLAIM_HALLOWED_GROUND);
		AddAdvertisedAction(INTERACTION_TYPE.CLEANSE_HALLOWED_GROUND);
		base.traitContainer.AddTrait(this, "Immovable");
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public HallowedGround(SaveDataTileObject data)
		: base(data)
	{
	}
}
