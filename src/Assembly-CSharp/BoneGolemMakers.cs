using System;
using Factions.Faction_Types;

[Serializable]
public class BoneGolemMakers : FactionIdeology
{
	public BoneGolemMakers()
		: base(FACTION_IDEOLOGY.Bone_Golem_Makers)
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
	}
}
