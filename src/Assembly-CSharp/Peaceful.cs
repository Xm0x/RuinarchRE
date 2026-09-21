using System;
using Factions.Faction_Types;

[Serializable]
public class Peaceful : FactionIdeology
{
	public Peaceful()
		: base(FACTION_IDEOLOGY.Peaceful)
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
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Warmonger, p_faction);
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Necromantic, p_faction);
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Blood_Sacrifices, p_faction);
	}
}
