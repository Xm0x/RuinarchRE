using Inner_Maps;

public class DestroyEyeWardData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY_EYE_WARD;

	public override string name => "Destroy Eye";

	public override string description => "This Action destroys a Demon Eye.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public DestroyEyeWardData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE_OBJECT };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is DemonEye demonEye)
		{
			LocationGridTile gridTileLocation = targetPOI.gridTileLocation;
			if (gridTileLocation != null)
			{
				GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Destroy_Explosion);
			}
			demonEye.ReduceHPBypassEverything(targetPOI.currentHP);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "player_intervention", LOG_TAG.Player);
			log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "destroyed");
			log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			if (UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == targetPOI)
			{
				UIManager.Instance.tileObjectInfoUI.CloseMenu();
			}
			base.ActivateAbility(targetPOI);
		}
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		if (tileObject.gridTileLocation == null)
		{
			return false;
		}
		if (tileObject.isBeingCarriedBy != null)
		{
			return false;
		}
		return base.CanPerformAbilityTowards(tileObject);
	}
}
