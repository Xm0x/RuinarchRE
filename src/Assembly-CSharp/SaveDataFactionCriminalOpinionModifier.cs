public class SaveDataFactionCriminalOpinionModifier : SaveDataSharedOpinionModifier
{
	public CRIME_TYPE crimeType;

	public CRIME_SEVERITY crimeSeverity;

	public override void Save(SharedOpinionModifier data)
	{
		base.Save(data);
		FactionCriminalOpinionModifier factionCriminalOpinionModifier = data as FactionCriminalOpinionModifier;
		crimeType = factionCriminalOpinionModifier.crimeType;
		crimeSeverity = factionCriminalOpinionModifier.crimeSeverity;
	}

	public override SharedOpinionModifier Load()
	{
		return new FactionCriminalOpinionModifier(this);
	}
}
