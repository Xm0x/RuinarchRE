using System;
using Factions.Faction_Types;

[Serializable]
public class HatesVampires : FactionIdeology
{
	public HatesVampires()
		: base(FACTION_IDEOLOGY.Hates_Vampires)
	{
	}

	public override bool DoesCharacterFitIdeology(Character character)
	{
		return true;
	}

	public override bool DoesCharacterFitIdeology(PreCharacterData character)
	{
		return true;
	}

	protected override void OnAddIdeology(FactionType factionType, Faction p_faction)
	{
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Reveres_Vampires, p_faction);
		factionType.AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
	}
}
