using UnityEngine;

public class SpireData : DemonicStructurePlayerSkill
{
	public override string name => "Spire";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPIRE;

	public override string description => "This Structure allows the Player to upgrade their Powers.";

	public override Vector2Int size => new Vector2Int(4, 7);

	public SpireData()
	{
		base.structureType = STRUCTURE_TYPE.SPIRE;
	}
}
