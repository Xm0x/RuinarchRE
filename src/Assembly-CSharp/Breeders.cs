using System;

[Serializable]
public class Breeders : FactionIdeology
{
	public Breeders()
		: base(FACTION_IDEOLOGY.Breeders)
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
