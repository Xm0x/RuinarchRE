public class SaveDataMonsterEgg : SaveDataTileObject
{
	public SUMMON_TYPE summonType;

	public string faction;

	public string settlement;

	public string structure;

	public GameDate hatchDate;

	public bool isSupposedToHatch;

	public bool hasHatched;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		MonsterEgg monsterEgg = tileObject as MonsterEgg;
		summonType = monsterEgg.summonType;
		faction = monsterEgg.faction?.persistentID;
		settlement = monsterEgg.settlement?.persistentID;
		structure = monsterEgg.structure?.persistentID;
		hatchDate = monsterEgg.hatchDate;
		isSupposedToHatch = monsterEgg.isSupposedToHatch;
		hasHatched = monsterEgg.hasHatched;
	}
}
