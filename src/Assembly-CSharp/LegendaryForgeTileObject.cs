using UnityEngine;

public class LegendaryForgeTileObject : TileObject
{
	public override Vector2 selectableSize => new Vector2(3f, 3f);

	public LegendaryForgeTileObject()
	{
		Initialize(TILE_OBJECT_TYPE.LEGENDARY_FORGE_TILE_OBJECT);
		AddAdvertisedAction(INTERACTION_TYPE.CLAIM_LEGENDARY_FORGE);
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_LEGENDARY_EQUIPMENT);
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public LegendaryForgeTileObject(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT, broadcastSignal);
	}
}
