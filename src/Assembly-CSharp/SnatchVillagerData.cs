using System.Linq;
using Inner_Maps.Location_Structures;

public class SnatchVillagerData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH_VILLAGER;

	public override string name => "Snatch Villager";

	public override string description => "Capture and imprison a Villager. It is easier to Snatch a Villager while it is alone and sleeping.";

	public override bool canBeCastOnBlessed => true;

	public SnatchVillagerData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.STRUCTURE,
			SPELL_TARGET.CHARACTER
		};
	}

	public override bool CanPerformAbilityTowards(LocationStructure target)
	{
		bool flag = false;
		if (target.partyStructureComponent != null)
		{
			if (target.partyStructureComponent.IsAvailable())
			{
				flag = true;
			}
			if (!target.partyStructureComponent.HasValidStoredTarget(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager))
			{
				flag = false;
			}
			if (target.partyStructureComponent.party != null)
			{
				flag = false;
			}
			if (target.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && !target.partyStructureComponent.HasValidResidents())
			{
				flag = false;
			}
		}
		return base.CanPerformAbilityTowards(target) && flag;
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		if (tileObject is MonsterSpawner monsterSpawner)
		{
			LocationStructure locationStructure = monsterSpawner.gridTileLocation?.structure;
			if (locationStructure != null)
			{
				return CanPerformAbilityTowards(locationStructure);
			}
		}
		return base.CanPerformAbilityTowards(tileObject);
	}

	public override bool CanPerformAbilityTowards(Character target)
	{
		bool flag = false;
		if (!PlayerManager.Instance.player.partyStructureDataHandler.HasPartyStructureAlreadyTargeting(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager, target) && PlayerManager.Instance.player.partyStructureDataHandler.HasAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager))
		{
			flag = true;
		}
		if (target.traitContainer.HasTrait("Sturdy"))
		{
			flag = false;
		}
		if (target.isDead)
		{
			flag = false;
		}
		return base.CanPerformAbilityTowards(target) && flag;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (flag)
		{
			if (target is LocationStructure { partyStructureComponent: not null } locationStructure)
			{
				if (locationStructure.partyStructureComponent.availableBehaviours.Contains(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager))
				{
					if (!(locationStructure is TortureChambers))
					{
						return locationStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER);
					}
					return true;
				}
				return false;
			}
			if (target is Character character)
			{
				if (PlayerManager.Instance.player.partyStructureDataHandler.HasStructureCapableOfPartyBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager))
				{
					if (character.faction != null && character.faction.isPlayerFaction)
					{
						return false;
					}
					bool flag2 = character.isNormalCharacter && character.race != RACE.RATMAN;
					if (!flag2 && character.race == RACE.DEMON && character.faction != null && character.faction.factionType.type != FACTION_TYPE.Demons)
					{
						flag2 = true;
					}
					if (!flag2)
					{
						return false;
					}
					LocationStructure currentStructure = character.currentStructure;
					if (currentStructure != null && (currentStructure.structureType == STRUCTURE_TYPE.KENNEL || currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS))
					{
						return character.gridTileLocation.HasDifferentStructureNeighbour(useFourNeighbours: true);
					}
					return true;
				}
				return false;
			}
			if (target is MonsterSpawner monsterSpawner && monsterSpawner.gridTileLocation?.structure != null && monsterSpawner.gridTileLocation.structure.partyStructureComponent != null)
			{
				return monsterSpawner.gridTileLocation.structure.partyStructureComponent.availableBehaviours.Contains(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager);
			}
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure structure)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(structure);
		if (structure.partyStructureComponent != null)
		{
			if (!structure.partyStructureComponent.IsAvailable())
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_No_Prison") + "|";
			}
			if (!structure.partyStructureComponent.HasValidStoredTarget(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager))
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("No_Valid_Stored_Targets") + "|";
			}
			if (structure.partyStructureComponent.party != null)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Active_Snatch_Party") + "|";
			}
			if (structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && !structure.partyStructureComponent.HasValidResidents())
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Attack_Village_No_Monster", structure) + "|";
			}
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

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character target)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(target);
		if (PlayerManager.Instance.player.partyStructureDataHandler.HasPartyStructureAlreadyTargeting(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager, target))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_Character_Already_Target") + "|";
		}
		if (!PlayerManager.Instance.player.partyStructureDataHandler.HasAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_No_Prison") + "|";
		}
		if (target.traitContainer.HasTrait("Sturdy"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_Sturdy") + "|";
		}
		if (target.isDead)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_Dead") + "|";
		}
		return text;
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		if (structure.partyStructureComponent != null)
		{
			UIManager.Instance.ShowSnatchVillagerUI(structure);
		}
		base.ActivateAbility(structure);
	}

	public override void ActivateAbility(IPointOfInterest target)
	{
		if (target is Character additionalTarget)
		{
			LocationStructure firstAvailableStructureForBehaviour = PlayerManager.Instance.player.partyStructureDataHandler.GetFirstAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager);
			if (firstAvailableStructureForBehaviour.partyStructureComponent != null)
			{
				UIManager.Instance.ShowSnatchVillagerUI(firstAvailableStructureForBehaviour, additionalTarget);
			}
		}
		else if (target is MonsterSpawner monsterSpawner)
		{
			LocationStructure structure = monsterSpawner.gridTileLocation.structure;
			if (structure.partyStructureComponent != null)
			{
				UIManager.Instance.ShowSnatchVillagerUI(structure);
			}
		}
		base.ActivateAbility(target);
	}
}
