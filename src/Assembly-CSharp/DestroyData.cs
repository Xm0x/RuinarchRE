using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class DestroyData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY;

	public override string name => "Destroy";

	public override string description => "This Ability instantly destroys an object.\nDestroying a resource pile from a Village city center will produce a Chaos Orb.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public DestroyData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE_OBJECT };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		LocationGridTile gridTileLocation = targetPOI.gridTileLocation;
		if (gridTileLocation != null)
		{
			GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Destroy_Explosion);
		}
		targetPOI.AdjustHP(-targetPOI.currentHP, ELEMENTAL_TYPE.Normal, triggerDeath: true, null, null, showHPBar: false, 0f, isPlayerSource: true);
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
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(this);
		if (damageBaseOnLevel > 0)
		{
			float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(this);
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			gridTileLocation.PopulateTilesInRadius(list, PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.DESTROY));
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile = list[i];
				if (locationGridTile == null)
				{
					continue;
				}
				GameManager.Instance.CreateParticleEffectAt(locationGridTile, PARTICLE_EFFECT.Destroy_Explosion);
				for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
				{
					Character character = locationGridTile.charactersHere[j];
					if (!character.isDead)
					{
						character.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Normal, triggerDeath: true, null, null, showHPBar: true, pierceBasedOnCurrentLevel, isPlayerSource: true);
						character.OnCharacterHitByPlayerSpell(-damageBaseOnLevel);
						if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
						{
							character.skillCauseOfDeath = PLAYER_SKILL_TYPE.DESTROY;
						}
					}
				}
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		base.ActivateAbility(targetPOI);
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

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is IPointOfInterest pointOfInterest && pointOfInterest.traitContainer.HasTrait("Indestructible"))
		{
			return false;
		}
		return base.IsValid(target);
	}
}
