using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class HarassVillagersData : RaidData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HARASS_VILLAGERS;

	public override string name => "Harass Villagers";

	public override bool shouldShowOnContextMenu => false;

	public HarassVillagersData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.STRUCTURE,
			SPELL_TARGET.SETTLEMENT
		};
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		UIManager.Instance.ShowRaidUI(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers);
		base.ActivateAbility(structure);
	}

	public override void ActivateAbility(BaseSettlement targetSettlement)
	{
		LocationStructure firstAvailableStructureForBehaviour = PlayerManager.Instance.player.partyStructureDataHandler.GetFirstAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers);
		UIManager.Instance.ShowRaidUI(firstAvailableStructureForBehaviour, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers, targetSettlement);
		base.ActivateAbility(targetSettlement);
	}

	public override void ActivateAbility(IPointOfInterest target)
	{
		if (target is MonsterSpawner monsterSpawner)
		{
			LocationStructure structure = monsterSpawner.gridTileLocation.structure;
			if (structure.partyStructureComponent != null)
			{
				UIManager.Instance.ShowRaidUI(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers);
			}
			base.ActivateAbility(target);
		}
	}
}
