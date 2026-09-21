using System.Collections.Generic;
using Object_Pools;
using UtilityScripts;

public class SpreadRumorData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPREAD_RUMOR;

	public override string name => "Spread Rumor";

	public override string description => "This Action instructs the character to spread a negative rumor about someone they know. Only available on Cultists.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public override bool canBeCastOnBlessed => true;

	public SpreadRumorData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		Character character = targetPOI as Character;
		if (character != null)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			character.PopulateListOfCultistTargets(list, (Character x) => x.isNormalCharacter && x.race.IsSapient() && !x.isDead);
			UIManager.Instance.ShowClickableObjectPicker(list, delegate(object o)
			{
				OnChooseCharacter(o, character);
			}, null, (Character t) => CanBeRumored(character, t), "", delegate(Character t)
			{
				OnHoverEnter(character, t);
			}, OnHoverExit, "", showCover: true, 25);
			RuinarchListPool<Character>.Release(list);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (!targetCharacter.limiterComponent.canPerform)
			{
				return false;
			}
			if (targetCharacter.traitContainer.HasTrait("Enslaved"))
			{
				return false;
			}
			if (targetCharacter.jobQueue.HasJob(JOB_TYPE.SPREAD_RUMOR) || targetCharacter.jobQueue.HasJob(JOB_TYPE.CULTIST_INSTRUCTION))
			{
				return false;
			}
			return !targetCharacter.isDead;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!targetCharacter.limiterComponent.canPerform)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Incapacitated") + "|";
		}
		if (targetCharacter.traitContainer.HasTrait("Enslaved"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Slave_Cannot_Perform") + "|";
		}
		if (targetCharacter.jobQueue.HasJob(JOB_TYPE.SPREAD_RUMOR))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Spreading_Rumors", targetCharacter) + "|";
		}
		if (targetCharacter.jobQueue.HasJob(JOB_TYPE.CULTIST_INSTRUCTION))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cultist_Already_Instructed", targetCharacter) + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (!PlayerSkillManager.Instance.selectedArchetype.IsPuppetmasterLoadout() && !PlayerSkillManager.Instance.unlockAllSkills)
		{
			return false;
		}
		return base.IsValid(target);
	}

	private bool CanBeRumored(Character owner, Character target)
	{
		if (target.traitContainer.HasTrait("Demon Cultist"))
		{
			return false;
		}
		if (owner.relationshipContainer.HasOpinionLabelWithCharacter(target, "Close Friend"))
		{
			return false;
		}
		return true;
	}

	private void OnHoverEnter(Character owner, Character target)
	{
		if (target.traitContainer.HasTrait("Demon Cultist"))
		{
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, Utilities.ColorizeInvalidText("Cannot target Cultists."));
			return;
		}
		if (owner.relationshipContainer.HasOpinionLabelWithCharacter(target, "Close Friend"))
		{
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, Utilities.ColorizeInvalidText("Cannot target Close Friends."));
			return;
		}
		string relationshipSummary = owner.visuals.GetRelationshipSummary(target);
		if (!string.IsNullOrEmpty(relationshipSummary))
		{
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, relationshipSummary);
		}
	}

	private void OnHoverExit(Character target)
	{
		PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
	}

	private void OnChooseCharacter(object obj, Character actor)
	{
		if (!(obj is Character character))
		{
			return;
		}
		UIManager.Instance.HideObjectPicker();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "instructed_spread_rumor", LOG_TAG.Crimes, LOG_TAG.Player);
		log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
		LogPool.Release(log);
		Character randomSpreadRumorOrNegativeInfoTarget = actor.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(character);
		if (randomSpreadRumorOrNegativeInfoTarget != null)
		{
			Rumor rumor = actor.rumorComponent.GenerateNewRandomRumor(randomSpreadRumorOrNegativeInfoTarget, character);
			if (rumor != null)
			{
				if (!actor.jobComponent.CreateSpreadRumorJob(randomSpreadRumorOrNegativeInfoTarget, rumor, JOB_TYPE.CULTIST_INSTRUCTION))
				{
					Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "spread_rumor_fail", LOG_TAG.Player);
					log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log2.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
					log2.AddLogToDatabase();
					PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
					base.ActivateAbility((IPointOfInterest)actor);
				}
			}
			else
			{
				Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "no_rumor_spread_rumor", LOG_TAG.Player);
				log3.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log3.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log3.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log3, releaseLogAfter: true);
			}
		}
		else
		{
			Log log4 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "no_target_spread_rumor", LOG_TAG.Player);
			log4.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log4.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log4.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log4, releaseLogAfter: true);
		}
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)actor);
	}
}
