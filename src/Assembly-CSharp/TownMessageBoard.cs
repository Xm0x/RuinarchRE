using UnityEngine;

public class TownMessageBoard : TileObject
{
	public override Vector2 selectableSize => new Vector2(2f, 1f);

	public TownMessageBoard()
	{
		Initialize(TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public TownMessageBoard(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT, broadcastSignal);
	}
}
