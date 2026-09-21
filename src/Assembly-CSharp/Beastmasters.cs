using System;

[Serializable]
public class Beastmasters : FactionIdeology
{
	public Beastmasters()
		: base(FACTION_IDEOLOGY.Beastmasters)
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
