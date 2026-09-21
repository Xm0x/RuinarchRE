using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class ClearVillageData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CLEAR_VILLAGE;

	public override string name => "Clear Village";

	public override string description => "This Ability wipes out all structures of an abandoned village.";

	public ClearVillageData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.SETTLEMENT,
			SPELL_TARGET.STRUCTURE
		};
	}

	public override void ActivateAbility(BaseSettlement targetSettlement)
	{
		if (targetSettlement is NPCSettlement nPCSettlement)
		{
			nPCSettlement.expirationComponent.CancelCurrentExpiry();
			nPCSettlement.DestroySettlement();
			base.ActivateAbility(targetSettlement);
		}
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		if (targetStructure.settlementLocation != null)
		{
			ActivateAbility(targetStructure.settlementLocation);
		}
	}

	public override bool CanPerformAbilityTowards(BaseSettlement targetSettlement)
	{
		if (targetSettlement is NPCSettlement nPCSettlement)
		{
			if (nPCSettlement.locationType != LOCATION_TYPE.VILLAGE)
			{
				return false;
			}
			if (nPCSettlement.HasResidents())
			{
				return false;
			}
		}
		return base.CanPerformAbilityTowards(targetSettlement);
	}

	public override bool CanPerformAbilityTowards(LocationStructure targetStructure)
	{
		if (targetStructure.settlementLocation != null)
		{
			return CanPerformAbilityTowards(targetStructure.settlementLocation);
		}
		return base.CanPerformAbilityTowards(targetStructure);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character)
		{
			return false;
		}
		if (target is NPCSettlement { isDestroyed: not false })
		{
			return false;
		}
		return base.IsValid(target);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(BaseSettlement p_targetSettlement)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(p_targetSettlement);
		if (p_targetSettlement is NPCSettlement nPCSettlement)
		{
			if (nPCSettlement.locationType != LOCATION_TYPE.VILLAGE)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Not_A_Village", p_targetSettlement) + "|";
			}
			if (nPCSettlement.HasResidents())
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Not_Abandoned_Village") + "|";
			}
		}
		return text;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure)
	{
		if (p_targetStructure.settlementLocation != null)
		{
			return GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure.settlementLocation);
		}
		return base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
	}
}
