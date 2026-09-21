using UnityEngine;

public class PrimordialPoolData : DemonicStructurePlayerSkill
{
	public override string name => "Primordial Pool";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PRIMORDIAL_POOL;

	public override string description => "Primordial pool sctructure";

	public override Vector2Int size => new Vector2Int(3, 3);

	public PrimordialPoolData()
	{
		base.structureType = STRUCTURE_TYPE.PRIMORDIAL_POOL;
	}
}
