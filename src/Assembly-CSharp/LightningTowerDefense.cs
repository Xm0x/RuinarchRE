using System;

[Serializable]
public class LightningTowerDefense : FactionIdeology
{
	public LightningTowerDefense()
		: base(FACTION_IDEOLOGY.Lightning_Tower_Defense)
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
