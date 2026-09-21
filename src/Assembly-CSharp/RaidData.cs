using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class RaidData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAID;

	public override string name => "Raid";

	public override string description => "Raid a village.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.RAID;

	public RaidData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.STRUCTURE,
			SPELL_TARGET.SETTLEMENT
		};
	}

	public override bool CanPerformAbilityTowards(BaseSettlement targetSettlement)
	{
		bool flag = base.CanPerformAbilityTowards(targetSettlement);
		if (type == PLAYER_SKILL_TYPE.RAID)
		{
			return flag;
		}
		if (flag)
		{
			if (!targetSettlement.HasResidents())
			{
				return false;
			}
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR specificStructurePartyBehaviour = GetSpecificStructurePartyBehaviour();
			if (!PlayerManager.Instance.player.partyStructureDataHandler.HasAvailableStructureForBehaviour(specificStructurePartyBehaviour))
			{
				return false;
			}
			if (PlayerManager.Instance.player.partyStructureDataHandler.HasPartyStructureAlreadyTargeting(specificStructurePartyBehaviour, targetSettlement))
			{
				return false;
			}
		}
		return flag;
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		if (tileObject is MonsterSpawner monsterSpawner)
		{
			LocationStructure structure = monsterSpawner.gridTileLocation.structure;
			return CanPerformAbilityTowards(structure);
		}
		return base.CanPerformAbilityTowards(tileObject);
	}

	public override bool CanPerformAbilityTowards(LocationStructure targetStructure)
	{
		bool flag = base.CanPerformAbilityTowards(targetStructure);
		if (type == PLAYER_SKILL_TYPE.RAID)
		{
			return flag;
		}
		if (flag && targetStructure.partyStructureComponent != null)
		{
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR specificStructurePartyBehaviour = GetSpecificStructurePartyBehaviour();
			if (!targetStructure.partyStructureComponent.HasValidStoredTarget(specificStructurePartyBehaviour))
			{
				return false;
			}
			if (targetStructure.partyStructureComponent.party != null)
			{
				return false;
			}
			if (targetStructure.structureType != STRUCTURE_TYPE.MARAUD && !targetStructure.partyStructureComponent.HasValidResidents())
			{
				return false;
			}
		}
		return flag;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (type == PLAYER_SKILL_TYPE.RAID)
		{
			return HasValidRaidActions();
		}
		if (base.IsValid(target))
		{
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR specificStructurePartyBehaviour = GetSpecificStructurePartyBehaviour();
			if (target is BaseSettlement baseSettlement)
			{
				if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE)
				{
					return PlayerManager.Instance.player.partyStructureDataHandler.HasStructureCapableOfPartyBehaviour(specificStructurePartyBehaviour);
				}
				return false;
			}
			if (target is LocationStructure locationStructure)
			{
				if (locationStructure.partyStructureComponent != null && locationStructure.partyStructureComponent.IsBehaviourAvailable(specificStructurePartyBehaviour))
				{
					if (!(locationStructure is Maraud))
					{
						return locationStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER);
					}
					return true;
				}
				return false;
			}
			if (target is MonsterSpawner monsterSpawner && monsterSpawner.gridTileLocation?.structure != null && monsterSpawner.gridTileLocation.structure.partyStructureComponent != null)
			{
				return monsterSpawner.gridTileLocation.structure.partyStructureComponent.IsBehaviourAvailable(specificStructurePartyBehaviour);
			}
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(BaseSettlement settlement)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(settlement);
		if (!settlement.HasResidents())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Raid_No_Residents") + "|";
		}
		SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR specificStructurePartyBehaviour = GetSpecificStructurePartyBehaviour();
		if (!PlayerManager.Instance.player.partyStructureDataHandler.HasAvailableStructureForBehaviour(specificStructurePartyBehaviour))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Raid_No_Maraud") + "|";
		}
		if (PlayerManager.Instance.player.partyStructureDataHandler.HasPartyStructureAlreadyTargeting(specificStructurePartyBehaviour, settlement))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Raid_Already_Target") + "|";
		}
		return text;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject)
	{
		if (targetTileObject is MonsterSpawner monsterSpawner)
		{
			LocationStructure structure = monsterSpawner.gridTileLocation.structure;
			return GetReasonsWhyCannotPerformAbilityTowards(structure);
		}
		return base.GetReasonsWhyCannotPerformAbilityTowards(targetTileObject);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
		if (type == PLAYER_SKILL_TYPE.RAID)
		{
			return text;
		}
		if (p_targetStructure.partyStructureComponent != null)
		{
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR specificStructurePartyBehaviour = GetSpecificStructurePartyBehaviour();
			if (!p_targetStructure.partyStructureComponent.HasValidStoredTarget(specificStructurePartyBehaviour))
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("No_Valid_Stored_Targets") + "|";
			}
			if (p_targetStructure.partyStructureComponent.party != null)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Active_Raid_Party") + "|";
			}
			if (p_targetStructure.structureType != STRUCTURE_TYPE.MARAUD && !p_targetStructure.partyStructureComponent.HasValidResidents())
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Attack_Village_No_Monster", p_targetStructure) + "|";
			}
		}
		return text;
	}

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (type == PLAYER_SKILL_TYPE.RAID && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null)
		{
			p_contextMenuItems.Clear();
			List<PLAYER_SKILL_TYPE> raidActions = PlayerManager.Instance.player.playerSkillComponent.raidActions;
			for (int i = 0; i < raidActions.Count; i++)
			{
				PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = raidActions[i];
				if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.RAID && PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE) is PlayerAction playerAction && playerAction.IsValid(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget))
				{
					p_contextMenuItems.Add(playerAction);
				}
			}
			return p_contextMenuItems;
		}
		return null;
	}

	private bool HasValidRaidActions()
	{
		List<PLAYER_SKILL_TYPE> raidActions = PlayerManager.Instance.player.playerSkillComponent.raidActions;
		for (int i = 0; i < raidActions.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = raidActions[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.RAID && PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE) is PlayerAction playerAction && playerAction.IsValid(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget))
			{
				return true;
			}
		}
		return false;
	}

	private SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR GetSpecificStructurePartyBehaviour()
	{
		return type switch
		{
			PLAYER_SKILL_TYPE.DESTROY_SUPPLIES => SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies, 
			PLAYER_SKILL_TYPE.DESTROY_STRUCTURES => SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Structures, 
			PLAYER_SKILL_TYPE.HARASS_VILLAGERS => SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers, 
			PLAYER_SKILL_TYPE.DESTROY_DEFENSES => SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
