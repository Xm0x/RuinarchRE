using UnityEngine;

public class PrismData : DemonicStructurePlayerSkill
{
	public override string name => "Prism";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PRISM;

	public override string description => "This Structure allows the Player to enhance Demon Cults and Cultists.";

	public override Vector2Int size => new Vector2Int(4, 4);

	public PrismData()
	{
		base.structureType = STRUCTURE_TYPE.PRISM;
	}
}
