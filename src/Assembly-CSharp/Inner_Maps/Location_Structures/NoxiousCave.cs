namespace Inner_Maps.Location_Structures;

public class NoxiousCave : ManMadeStructure
{
	public NoxiousCave(Region location)
		: base(STRUCTURE_TYPE.NOXIOUS_CAVE, location)
	{
	}

	public NoxiousCave(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
	}

	public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource)
	{
		base.OnTileDamaged(tile, amount, isPlayerSource);
		AdjustHP(amount, null, isPlayerSource);
		OnStructureDamaged();
	}

	public override bool DoesTileContributeToDamage(LocationGridTile tile)
	{
		return true;
	}
}
