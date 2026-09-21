using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class ZapData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ZAP;

	public override string name => "Zap";

	public override string description => "This Ability can be used to apply Zapped on a character - temporarily preventing movement.\nZapping a hostile Villager will produce a Chaos Orb.";

	public ZapData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.CHARACTER,
			SPELL_TARGET.TILE_OBJECT
		};
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.ZAP);
		targetPOI.traitContainer.AddTrait(targetPOI, "Zapped", null, bypassElementalChance: true, durationBonusPerLevel, 0f, ELEMENTAL_TYPE.Electric);
		targetPOI.traitContainer.GetTraitOrStatus<Zapped>("Zapped")?.SetIsPlayerSource(p_state: true);
		if (targetPOI is Character character)
		{
			SkillData playerActionData = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.ZAP);
			if (character.crimeComponent.witnessedCrimes.Count > 0 && playerActionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Forget_Witnessed_Crimes))
			{
				List<CrimeData> list = RuinarchListPool<CrimeData>.Claim(character.crimeComponent.witnessedCrimes.Count);
				list.AddRange(character.crimeComponent.witnessedCrimes);
				for (int i = 0; i < character.crimeComponent.witnessedCrimes.Count; i++)
				{
					CrimeData crimeData = character.crimeComponent.witnessedCrimes[i];
					if (!character.crimeComponent.reportedCrimes.Contains(crimeData))
					{
						crimeData.RemoveWitnessFromZap(character);
					}
				}
				RuinarchListPool<CrimeData>.Release(list);
			}
		}
		if (UIManager.Instance.characterInfoUI.isShowing)
		{
			UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "player_intervention", LOG_TAG.Player);
		log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Zapped_Verb"), LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		Messenger.Broadcast(PlayerSkillSignals.ZAP_ACTIVATED, targetPOI);
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.characterClass.className == "Barbarian")
		{
			return false;
		}
		if (targetCharacter.isDead || !targetCharacter.carryComponent.IsNotBeingCarried() || targetCharacter.traitContainer.HasTrait("Zapped", "Electric"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.characterClass.className == "Barbarian")
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Zap_Immune_Barbarian") + "|";
		}
		return text;
	}
}
