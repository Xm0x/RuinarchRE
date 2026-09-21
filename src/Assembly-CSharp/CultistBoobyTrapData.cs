using System.Collections.Generic;
using UtilityScripts;

public class CultistBoobyTrapData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP;

	public override string name => "Booby Trap";

	public override string description => "This Ability instructs the character to Booby Trap an object owned by someone they know. Only available on Cultists.";

	public override bool canBeCastOnBlessed => true;

	public CultistBoobyTrapData()
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
			}, null, (Character t) => CanBeTrapped(character, t), "", delegate(Character t)
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
			return !targetCharacter.isDead;
		}
		return false;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (!PlayerSkillManager.Instance.selectedArchetype.IsRavagerLoadout() && !PlayerSkillManager.Instance.unlockAllSkills)
		{
			return false;
		}
		return base.IsValid(target);
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
		return text;
	}

	private bool CanBeTrapped(Character owner, Character target)
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
		if (obj is Character character)
		{
			UIManager.Instance.HideObjectPicker();
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "instructed_trap", LOG_TAG.Crimes, LOG_TAG.Player);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			if (!actor.jobComponent.CreatePlaceTrapJob(character, JOB_TYPE.CULTIST_INSTRUCTION))
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "cultist_no_trap_target", LOG_TAG.Player);
				log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
			}
			base.ActivateAbility((IPointOfInterest)actor);
		}
	}
}
