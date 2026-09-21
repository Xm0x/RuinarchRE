using System;

public class SkinnableAnimal : Summon
{
	public int count { get; set; }

	public override Type serializedData => typeof(SaveDataSkinnableAnimal);

	public SkinnableAnimal(SUMMON_TYPE summonType, string className, RACE race, GENDER gender)
		: base(summonType, className, race, gender)
	{
		count = 80;
	}

	public SkinnableAnimal(SaveDataSkinnableAnimal data)
		: base(data)
	{
		count = data.count;
	}
}
