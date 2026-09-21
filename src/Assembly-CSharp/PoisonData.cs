using Traits;

public class PoisonData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.POISON;

	public override string name => "Poison";

	public override string description => "This Ability applies Poison on an object.";

	public PoisonData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE_OBJECT };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.POISON);
		targetPOI.traitContainer.AddTrait(targetPOI, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
		targetPOI.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.SetIsPlayerSource(p_state: true);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", name + " activated", LOG_TAG.Player);
		log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		if ((tileObject.gridTileLocation == null && tileObject.isBeingCarriedBy == null) || tileObject.traitContainer.HasTrait("Poisoned", "Robust"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(tileObject);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target) || target is TileObject { isBeingCarriedBy: not null })
		{
			return true;
		}
		return false;
	}
}
