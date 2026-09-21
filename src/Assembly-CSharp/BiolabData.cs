using Inner_Maps;
using UnityEngine;

public class BiolabData : DemonicStructurePlayerSkill
{
	public override string name => "Biolab";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BIOLAB;

	public override string description => "The Biolab allows the player to afflict Villagers with a customizable Plague.";

	public override Vector2Int size => new Vector2Int(6, 6);

	public BiolabData()
	{
		base.structureType = STRUCTURE_TYPE.BIOLAB;
	}

	protected override string InvalidMessage(LocationGridTile tile)
	{
		if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB))
		{
			return LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_one_biolab");
		}
		return base.InvalidMessage(tile);
	}
}
