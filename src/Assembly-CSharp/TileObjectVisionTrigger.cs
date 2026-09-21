using System.Linq;
using Traits;

public class TileObjectVisionTrigger : POIVisionTrigger
{
	public override void Initialize(IDamageable damageable)
	{
		base.Initialize(damageable);
		if (base.poi is GenericTileObject && projectileReceiver != null)
		{
			projectileReceiver.gameObject.SetActive(value: false);
		}
		TileObject tileObject = damageable as TileObject;
		int num = 0;
		if (tileObject.tileObjectType.IsTileObjectVisibleByDefault())
		{
			num++;
		}
		if (tileObject.lastManipulatedBy is Player)
		{
			num++;
		}
		num += tileObject.allJobsTargetingThis.Count;
		num += tileObject.traitContainer.statuses.Count((Status s) => s.isTangible);
		SetFilterVotes(num);
	}

	public override bool IgnoresStructureDifference()
	{
		if (base.poi is MovingTileObject)
		{
			return true;
		}
		if (base.poi is TileObject tileObject && tileObject.tileObjectType.IsDemonicStructureTileObject())
		{
			return true;
		}
		return false;
	}

	public override bool IgnoresRoomDifference()
	{
		if (base.poi is TileObject tileObject && tileObject.tileObjectType.IsDemonicStructureTileObject())
		{
			return true;
		}
		return false;
	}
}
