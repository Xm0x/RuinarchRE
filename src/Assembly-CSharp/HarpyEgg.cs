using System;

public class HarpyEgg : MonsterEgg
{
	public override Type serializedData => typeof(SaveDataHarpyEgg);

	public HarpyEgg()
		: base(TILE_OBJECT_TYPE.HARPY_EGG, SUMMON_TYPE.Harpy, GameManager.Instance.GetTicksBasedOnHour(1))
	{
	}

	public HarpyEgg(SaveDataHarpyEgg data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Harpy Egg " + base.id;
	}
}
