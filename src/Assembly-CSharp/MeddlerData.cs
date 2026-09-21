using Inner_Maps;
using UnityEngine;

public class MeddlerData : DemonicStructurePlayerSkill
{
	public override string name => "Meddler";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MEDDLER;

	public override string description => "This Structure allows the player to start various Schemes.";

	public override Vector2Int size => new Vector2Int(5, 7);

	public MeddlerData()
	{
		base.structureType = STRUCTURE_TYPE.MEDDLER;
	}

	protected override string InvalidMessage(LocationGridTile tile)
	{
		if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER))
		{
			return LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_one_meddler");
		}
		return base.InvalidMessage(tile);
	}
}
