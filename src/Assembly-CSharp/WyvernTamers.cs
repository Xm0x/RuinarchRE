using System;

[Serializable]
public class WyvernTamers : FactionIdeology
{
	public WyvernTamers()
		: base(FACTION_IDEOLOGY.Wyvern_Tamers)
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
