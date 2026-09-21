namespace Factions.Faction_Types;

public class Vagrants : FactionType
{
	public override RESOURCE mainResource => RESOURCE.WOOD;

	public Vagrants()
		: base(FACTION_TYPE.Vagrants)
	{
	}

	public Vagrants(SaveDataFactionType saveData)
		: base(FACTION_TYPE.Vagrants, saveData)
	{
	}

	public override void SetAsDefault(Faction p_faction)
	{
		base.hasCrimes = true;
		AddCrime(CRIME_TYPE.Infidelity, CRIME_SEVERITY.Infraction);
		AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Disturbances, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Arson, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
		AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
	}

	public override void SetFixedData()
	{
	}

	public override CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType)
	{
		return crimeType switch
		{
			CRIME_TYPE.Infidelity => CRIME_SEVERITY.Infraction, 
			CRIME_TYPE.Theft => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Disturbances => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Assault => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Arson => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Murder => CRIME_SEVERITY.Serious, 
			CRIME_TYPE.Cannibalism => CRIME_SEVERITY.Serious, 
			CRIME_TYPE.Werewolf => CRIME_SEVERITY.Heinous, 
			CRIME_TYPE.Vampire => CRIME_SEVERITY.Heinous, 
			CRIME_TYPE.Demon_Worship => CRIME_SEVERITY.Heinous, 
			_ => CRIME_SEVERITY.None, 
		};
	}
}
