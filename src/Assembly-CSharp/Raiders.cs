using System;

[Serializable]
public class Raiders : FactionIdeology
{
	public Raiders()
		: base(FACTION_IDEOLOGY.Raiders)
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
