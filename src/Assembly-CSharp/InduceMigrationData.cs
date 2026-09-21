using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class InduceMigrationData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INDUCE_MIGRATION;

	public override string name => "Induce Migration";

	public override string description => "This Ability forces a migration event of new residents to the target Village.";

	public InduceMigrationData()
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
			nPCSettlement.migrationComponent.SetVillageMigrationMeter(0);
			nPCSettlement.migrationComponent.InduceMigrationEvent();
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "WorldEvents", "WorldEvents_Table", "induced_migration", LOG_TAG.Major);
			log.AddToFillers(nPCSettlement, nPCSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
			if (nPCSettlement.owner != null)
			{
				log.AddToFillers(nPCSettlement.owner, nPCSettlement.owner.name, LOG_IDENTIFIER.FACTION_1);
			}
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
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
			if (!nPCSettlement.migrationComponent.IsMigrationEventAllowed())
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
		if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
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
			if (!nPCSettlement.migrationComponent.IsMigrationEventAllowed())
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Trigger_Migration", p_targetSettlement) + "|";
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
