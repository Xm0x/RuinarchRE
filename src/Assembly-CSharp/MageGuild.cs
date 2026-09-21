using System;

[Serializable]
public class MageGuild : FactionIdeology
{
	public MageGuild()
		: base(FACTION_IDEOLOGY.Mage_Guild)
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
}
