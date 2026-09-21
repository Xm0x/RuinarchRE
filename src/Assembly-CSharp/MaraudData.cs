using UnityEngine;

public class MaraudData : DemonicStructurePlayerSkill
{
	public override string name => "Maraud";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MARAUD;

	public override string description => "This Structure allows the player to summon Raid Parties. Raid Parties harass Villages and is primarily used to generate some Chaos Orbs.";

	public override Vector2Int size => new Vector2Int(5, 7);

	public MaraudData()
	{
		base.structureType = STRUCTURE_TYPE.MARAUD;
	}
}
