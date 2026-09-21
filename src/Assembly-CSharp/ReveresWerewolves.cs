using System;
using Factions.Faction_Types;

[Serializable]
public class ReveresWerewolves : FactionIdeology
{
	public ReveresWerewolves()
		: base(FACTION_IDEOLOGY.Reveres_Werewolves)
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
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Hates_Werewolves, p_faction);
		factionType.RemoveCrime(CRIME_TYPE.Werewolf);
	}
}
