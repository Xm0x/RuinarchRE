namespace Factions.Faction_Types;

public class Retaliator : FactionType
{
	public override RESOURCE mainResource => RESOURCE.WOOD;

	public override bool shouldShowFactionEmblem => false;

	public Retaliator()
		: base(FACTION_TYPE.Retaliator)
	{
	}

	public Retaliator(SaveDataFactionType saveData)
		: base(FACTION_TYPE.Retaliator, saveData)
	{
	}

	public override void SetAsDefault(Faction p_faction)
	{
	}

	public override void SetFixedData()
	{
	}

	public override CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType)
	{
		return CRIME_SEVERITY.Unapplicable;
	}
}
