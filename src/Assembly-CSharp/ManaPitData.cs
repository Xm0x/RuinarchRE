using UnityEngine;

public class ManaPitData : DemonicStructurePlayerSkill
{
	public override string name => "Mana Pit";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MANA_PIT;

	public override string description => "This Structure increases the player's maximum Mana capacity and hourly Mana regen.";

	public override Vector2Int size => new Vector2Int(4, 4);

	public ManaPitData()
	{
		base.structureType = STRUCTURE_TYPE.MANA_PIT;
	}
}
