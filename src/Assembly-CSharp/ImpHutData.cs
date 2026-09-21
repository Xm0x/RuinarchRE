using UnityEngine;

public class ImpHutData : DemonicStructurePlayerSkill
{
	public override string name => "Imp Hut";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.IMP_HUT;

	public override string description => "This Structure allows the Player to spawn Imps.";

	public override Vector2Int size => new Vector2Int(6, 5);

	public ImpHutData()
	{
		base.structureType = STRUCTURE_TYPE.IMP_HUT;
	}
}
