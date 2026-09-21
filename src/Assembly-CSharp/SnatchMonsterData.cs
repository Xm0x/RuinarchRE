using System.Linq;
using Inner_Maps.Location_Structures;

public class SnatchMonsterData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH_MONSTER;

	public override string name => "Snatch Monster";

	public override string description => "Snatch a monster and bring them to this structure.";

	public override bool canBeCastOnBlessed => true;

	public SnatchMonsterData()
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
		if (target is Kennel kennel)
		{
			if (kennel.partyStructureComponent.IsAvailable())
			{
				flag = true;
			}
			if (!kennel.partyStructureComponent.HasValidStoredTarget(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster))
			{
				flag = false;
			}
			if (kennel.partyStructureComponent.party != null)
			{
				flag = false;
			}
			if (target.structureType != STRUCTURE_TYPE.KENNEL && !target.partyStructureComponent.HasValidResidents())
			{
				flag = false;
			}
		}
		return base.CanPerformAbilityTowards(target) && flag;
	}

	public override bool CanPerformAbilityTowards(Character target)
	{
		bool flag = false;
		if (target is Summon summon)
		{
			if (!PlayerManager.Instance.player.partyStructureDataHandler.HasPartyStructureAlreadyTargeting(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster, summon) && PlayerManager.Instance.player.partyStructureDataHandler.HasAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster))
			{
				flag = true;
			}
			if (summon.traitContainer.HasTrait("Sturdy"))
			{
				flag = false;
			}
			if (summon.isDead)
			{
				flag = false;
			}
		}
		return base.CanPerformAbilityTowards(target) && flag;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target))
		{
			if (target is LocationStructure { partyStructureComponent: not null } locationStructure)
			{
				if (locationStructure.partyStructureComponent.availableBehaviours.Contains(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster))
				{
					if (!(locationStructure is Kennel))
					{
						return locationStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER);
					}
					return true;
				}
				return false;
			}
			if (target is Summon summon && PlayerManager.Instance.player.partyStructureDataHandler.HasStructureCapableOfPartyBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster))
			{
				if (summon.faction != null && summon.faction.isPlayerFaction)
				{
					return false;
				}
				LocationStructure currentStructure = summon.currentStructure;
				if (summon.currentStructure != null && (summon.currentStructure.structureType == STRUCTURE_TYPE.KENNEL || currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS))
				{
					return summon.gridTileLocation.HasDifferentStructureNeighbour(useFourNeighbours: true);
				}
				return true;
			}
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure structure)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(structure);
		if (structure.partyStructureComponent != null)
		{
			if (!structure.partyStructureComponent.IsAvailable())
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_No_Kennel") + "|";
			}
			if (!structure.partyStructureComponent.HasValidStoredTarget(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster))
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

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character target)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(target);
		if (PlayerManager.Instance.player.partyStructureDataHandler.HasPartyStructureAlreadyTargeting(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster, target))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_Already_Target") + "|";
		}
		if (!PlayerManager.Instance.player.partyStructureDataHandler.HasAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Snatch_No_Kennel") + "|";
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
			UIManager.Instance.ShowSnatchMonsterUI(structure);
		}
		base.ActivateAbility(structure);
	}

	public override void ActivateAbility(IPointOfInterest target)
	{
		if (target is Summon additionalTarget)
		{
			LocationStructure firstAvailableStructureForBehaviour = PlayerManager.Instance.player.partyStructureDataHandler.GetFirstAvailableStructureForBehaviour(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster);
			if (firstAvailableStructureForBehaviour != null && firstAvailableStructureForBehaviour.partyStructureComponent != null)
			{
				UIManager.Instance.ShowSnatchMonsterUI(firstAvailableStructureForBehaviour, additionalTarget);
			}
		}
		base.ActivateAbility(target);
	}
}
