using System;

[Serializable]
public class TowerDefense : FactionIdeology
{
	public TowerDefense()
		: base(FACTION_IDEOLOGY.Tower_Defense)
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
