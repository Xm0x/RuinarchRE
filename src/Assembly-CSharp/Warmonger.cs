using System;
using Factions.Faction_Types;

[Serializable]
public class Warmonger : FactionIdeology
{
	public Warmonger()
		: base(FACTION_IDEOLOGY.Warmonger)
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
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Peaceful, p_faction);
	}
}
