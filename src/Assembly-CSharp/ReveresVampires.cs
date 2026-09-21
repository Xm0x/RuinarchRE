using System;
using Factions.Faction_Types;

[Serializable]
public class ReveresVampires : FactionIdeology
{
	public ReveresVampires()
		: base(FACTION_IDEOLOGY.Reveres_Vampires)
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
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Hates_Vampires, p_faction);
		factionType.RemoveCrime(CRIME_TYPE.Vampire);
	}
}
