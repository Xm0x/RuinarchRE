using UnityEngine;

public class OstracizerData : DemonicStructurePlayerSkill
{
	public override string name => "Ostracizer";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.OSTRACIZER;

	public override string description => "This Structure allows the player to make most Villagers look down on a smaller and specific subset of people. This is a powerful tool in fracturing a large society.";

	public override Vector2Int size => new Vector2Int(1, 1);

	public OstracizerData()
	{
		base.structureType = STRUCTURE_TYPE.OSTRACIZER;
	}
}
