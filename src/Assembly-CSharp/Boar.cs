using UtilityScripts;

public class Boar : SkinnableAnimal
{
	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.BOAR_HIDE;

	public Boar()
		: base(SUMMON_TYPE.Boar, "Boar", RACE.BOAR, Utilities.GetRandomGender())
	{
	}

	public Boar(string className)
		: base(SUMMON_TYPE.Boar, className, RACE.BOAR, Utilities.GetRandomGender())
	{
	}

	public Boar(SaveDataSkinnableAnimal data)
		: base(data)
	{
	}
}
