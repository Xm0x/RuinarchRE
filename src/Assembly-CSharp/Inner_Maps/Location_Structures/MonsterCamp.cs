namespace Inner_Maps.Location_Structures;

public class MonsterCamp : ManMadeStructure
{
	public MonsterCamp(Region location)
		: base(STRUCTURE_TYPE.MONSTER_CAMP, location)
	{
		SetMaxHPAndReset(6000);
	}

	public MonsterCamp(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(6000);
	}
}
