using System;
using Factions.Faction_Types;

[Serializable]
public class HatesWerewolves : FactionIdeology
{
	public HatesWerewolves()
		: base(FACTION_IDEOLOGY.Hates_Werewolves)
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
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Reveres_Werewolves, p_faction);
		factionType.AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
	}
}
