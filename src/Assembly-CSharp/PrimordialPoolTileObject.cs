using System.Collections.Generic;
using UnityEngine;

public class PrimordialPoolTileObject : TileObject
{
	public override Vector2 selectableSize => new Vector2(3f, 3f);

	public override Vector3 worldPosition => mapVisual.visionTrigger.transform.position;

	public override Vector3 attackRangePosition => GetAttackRangePosForDemonicStructureTileObject();

	public PrimordialPoolTileObject()
	{
		Initialize(TILE_OBJECT_TYPE.PRIMORDIAL_POOL_TILE_OBJECT);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public PrimordialPoolTileObject(SaveDataTileObject data)
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
