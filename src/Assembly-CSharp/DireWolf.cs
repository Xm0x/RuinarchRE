using UtilityScripts;

public class DireWolf : SkinnableAnimal
{
	public override bool defaultDigMode => true;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.WOLF_HIDE;

	public DireWolf()
		: base(SUMMON_TYPE.Dire_Wolf, "Dire", RACE.WOLF, Utilities.GetRandomGender())
	{
	}

	public DireWolf(string className)
		: base(SUMMON_TYPE.Dire_Wolf, className, RACE.WOLF, Utilities.GetRandomGender())
	{
	}

	public DireWolf(SaveDataSkinnableAnimal data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetEnableDigging(state: true);
	}
}
