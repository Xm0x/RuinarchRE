using System.Collections.Generic;
using UnityEngine;

public class KennelTileObject : TileObject
{
	public override Vector2 selectableSize => new Vector2(3f, 3f);

	public override Vector3 worldPosition => mapVisual.visionTrigger.transform.position;

	public override Vector3 attackRangePosition => GetAttackRangePosForDemonicStructureTileObject();

	public KennelTileObject()
	{
		Initialize(TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public KennelTileObject(SaveDataTileObject data)
		: base(data)
	{
	}

	public override bool CanBeSelected()
	{
		return true;
	}

	public override bool IsCurrentlySelected()
	{
		return false;
	}

	public override void LeftSelectAction()
	{
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
