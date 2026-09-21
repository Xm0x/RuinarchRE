using Object_Pools;
using UnityEngine.Localization.Settings;

public class PlagueData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PLAGUE;

	public override string name => "Plague";

	public override string description => GetDescription();

	public override string localizedDescription => GetDescription();

	public override string afflictionTraitName => "Plagued";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public PlagueData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.CHARACTER,
			SPELL_TARGET.TILE_OBJECT
		};
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		string value = ((!(LocalizationSettings.SelectedLocale.Identifier.Code == "tr-TR")) ? TraitManager.Instance.GetLocalizedNameOfTrait("Plagued") : localizedName);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "player_afflicted", LOG_TAG.Player, LOG_TAG.Life_Changes);
		log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
		LogPool.Release(log);
		ApplyAfflictionEffects(targetPOI);
		OnExecutePlayerSkill();
	}

	public override void ApplyAfflictionEffects(IPointOfInterest target, int overridenDuration = 0)
	{
		if (target is Character character)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, character);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Robust", "Beast"))
		{
			return false;
		}
		if (!PlagueDisease.Instance.CanAddPlaguedStatusOnPOIBasedOnLifespan(targetCharacter, out var _))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.traitContainer.HasTrait("Robust"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Robust_Plague_Immune") + "|";
		}
		if (!PlagueDisease.Instance.CanAddPlaguedStatusOnPOIBasedOnLifespan(targetCharacter, out var _))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Plague_Immune", targetCharacter) + "|";
		}
		return text;
	}

	private string GetDescription()
	{
		string text = LocalizationManager.Instance.GetLocalizedValue("Afflictions_Table", name + "_Description") ?? "";
		if (GameManager.Instance.gameHasStarted)
		{
			text = text + "\n\n" + PlagueDisease.Instance.GetPlagueEffectsSummary();
		}
		return text;
	}
}
