using UnityEngine;

public class CryptData : DemonicStructurePlayerSkill
{
	public override string name => "Crypt";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CRYPT;

	public override string description => "This Structure allows the Player to spawn Skeletons.";

	public override Vector2Int size => new Vector2Int(6, 4);

	public CryptData()
	{
		base.structureType = STRUCTURE_TYPE.CRYPT;
	}
}
