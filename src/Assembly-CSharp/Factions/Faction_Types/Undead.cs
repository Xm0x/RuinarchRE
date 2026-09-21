namespace Factions.Faction_Types;

public class Undead : FactionType
{
	public override RESOURCE mainResource => RESOURCE.WOOD;

	public Undead()
		: base(FACTION_TYPE.Undead)
	{
	}

	public Undead(SaveDataFactionType saveData)
		: base(FACTION_TYPE.Undead, saveData)
	{
	}

	public override void SetAsDefault(Faction p_faction)
	{
		Warmonger ideology = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
		AddIdeology(ideology, p_faction);
	}

	public override void SetFixedData()
	{
	}

	public override CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType)
	{
		return CRIME_SEVERITY.Unapplicable;
	}
}
