namespace Inner_Maps.Location_Structures;

public class BanditCamp : ManMadeStructure
{
	public BanditCamp(Region location)
		: base(STRUCTURE_TYPE.BANDIT_CAMP, location)
	{
	}

	public BanditCamp(Region location, SaveDataManMadeStructure data)
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
