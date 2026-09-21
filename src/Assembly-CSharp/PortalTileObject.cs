using System.Collections.Generic;
using UnityEngine;

public class PortalTileObject : TileObject
{
	public override Vector2 selectableSize => new Vector2(4f, 3f);

	public override Vector3 worldPosition
	{
		get
		{
			Vector3 position = mapVisual.transform.position;
			position.x -= 0.5f;
			return position;
		}
	}

	public PortalTileObject()
	{
		Initialize(TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		AddAdvertisedAction(INTERACTION_TYPE.SEARCH_FOR_DEMONIC_AREA);
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public PortalTileObject(SaveDataTileObject data)
		: base(data)
	{
	}

	public override bool CanBeSelected()
	{
		return true;
	}

	public override void LeftSelectAction()
	{
		UIManager.Instance.ShowStructureInfo(gridTileLocation.structure);
	}

	public override void RightSelectAction()
	{
	}

	public override void MiddleSelectAction()
	{
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.actions = new List<PLAYER_SKILL_TYPE>();
	}
}
