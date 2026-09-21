using UnityEngine;

namespace Inner_Maps;

public class SaveDataGridTileTileObjectComponent : SaveData<GridTileTileObjectComponent>
{
	public string genericTileObjectID;

	public bool hasLandmine;

	public bool hasFreezingTrap;

	public bool hasSnareTrap;

	public bool isSeenByEyeWard;

	public bool isFreezingTrapPlayerSource;

	public bool isSnareTrapPlayerSource;

	public bool hasStampede;

	public int stampedeDuration;

	public Vector3 stampedeDirection;

	public float stampedeAngle;

	public RACE[] freezingTrapExclusions;

	public RACE snareTrapExclusion;

	public override void Save(GridTileTileObjectComponent data)
	{
		base.Save(data);
		genericTileObjectID = data.genericTileObject.persistentID;
		hasLandmine = data.hasLandmine;
		hasFreezingTrap = data.hasFreezingTrap;
		hasSnareTrap = data.hasSnareTrap;
		freezingTrapExclusions = data.freezingTrapExclusions;
		snareTrapExclusion = data.snareTrapExclusion;
		isSeenByEyeWard = data.isSeenByEyeWard;
		isFreezingTrapPlayerSource = data.isFreezingTrapPlayerSource;
		isSnareTrapPlayerSource = data.isSnareTrapPlayerSource;
	}

	public override GridTileTileObjectComponent Load()
	{
		return new GridTileTileObjectComponent(this);
	}

	public override void CleanUp()
	{
		freezingTrapExclusions = null;
	}
}
