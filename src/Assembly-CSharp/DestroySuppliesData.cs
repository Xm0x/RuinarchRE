using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class DestroySuppliesData : RaidData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY_SUPPLIES;

	public override string name => "Destroy Supplies";

	public override bool shouldShowOnContextMenu => false;

	public DestroySuppliesData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.STRUCTURE,
			SPELL_TARGET.SETTLEMENT
		};
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		UIManager.Instance.ShowRaidUI(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies);
		base.ActivateAbility(structure);
	}

	public override void ActivateAbility(BaseSettlement targetSettlement)
	{
		LocationStructure firstAvailableStructureForBehaviour = PlayerManager.Instance.player.partyStructureDataHandler.GetFirstAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies);
		UIManager.Instance.ShowRaidUI(firstAvailableStructureForBehaviour, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies, targetSettlement);
		base.ActivateAbility(targetSettlement);
	}

	public override void ActivateAbility(IPointOfInterest target)
	{
		if (target is MonsterSpawner monsterSpawner)
		{
			LocationStructure structure = monsterSpawner.gridTileLocation.structure;
			if (structure.partyStructureComponent != null)
			{
				UIManager.Instance.ShowRaidUI(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies);
			}
			base.ActivateAbility(target);
		}
	}
}
