using System;
using Factions.Faction_Types;

[Serializable]
public class NatureWorship : FactionIdeology
{
	public NatureWorship()
		: base(FACTION_IDEOLOGY.Nature_Worship)
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
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Divine_Worship, p_faction);
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Demon_Worship, p_faction);
		factionType.RemoveCrime(CRIME_TYPE.Nature_Worship);
	}
}
