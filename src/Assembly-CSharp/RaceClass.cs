public struct RaceClass
{
	public RACE race;

	public string className;

	public RaceClass(RACE race, string className)
	{
		this.race = race;
		this.className = className;
	}

	public override string ToString()
	{
		return race.ToStringEnumWithSpaceNormalized() + " " + className;
	}
}
