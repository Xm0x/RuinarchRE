using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class DestroyDefensesData : RaidData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY_DEFENSES;

	public override string name => "Destroy Defenses";

	public override bool shouldShowOnContextMenu => false;

	public DestroyDefensesData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.STRUCTURE,
			SPELL_TARGET.SETTLEMENT
		};
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		UIManager.Instance.ShowRaidUI(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses);
		base.ActivateAbility(structure);
	}

	public override void ActivateAbility(BaseSettlement targetSettlement)
	{
		LocationStructure firstAvailableStructureForBehaviour = PlayerManager.Instance.player.partyStructureDataHandler.GetFirstAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses);
		UIManager.Instance.ShowRaidUI(firstAvailableStructureForBehaviour, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses, targetSettlement);
		base.ActivateAbility(targetSettlement);
	}

	public override void ActivateAbility(IPointOfInterest target)
	{
		if (target is MonsterSpawner monsterSpawner)
		{
			LocationStructure structure = monsterSpawner.gridTileLocation.structure;
			if (structure.partyStructureComponent != null)
			{
				UIManager.Instance.ShowRaidUI(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses);
			}
			base.ActivateAbility(target);
		}
	}
}
