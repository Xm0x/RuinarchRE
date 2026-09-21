using UtilityScripts;

public class Bear : SkinnableAnimal
{
	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.BEAR_HIDE;

	public Bear()
		: base(SUMMON_TYPE.Bear, "Bear", RACE.BEAR, Utilities.GetRandomGender())
	{
	}

	public Bear(string className)
		: base(SUMMON_TYPE.Bear, className, RACE.BEAR, Utilities.GetRandomGender())
	{
	}

	public Bear(SaveDataSkinnableAnimal data)
		: base(data)
	{
	}
}
