using System;
using Factions.Faction_Types;

[Serializable]
public class Inclusive : FactionIdeology
{
	public Inclusive()
		: base(FACTION_IDEOLOGY.Inclusive)
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
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive, p_faction);
	}
}
