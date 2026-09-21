using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class EvangelizeData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EVANGELIZE;

	public override string name => "Preach";

	public override string description => "This Ability instructs the character to preach about Demon Worship to someone they know. Only available on Cultists.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public override bool canBeCastOnBlessed => true;

	public EvangelizeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		Character character = targetPOI as Character;
		if (character == null)
		{
			return;
		}
		if (character.characterClass.className == "Demon Cult Leader")
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
			{
				Character character2 = CharacterManager.Instance.allCharacters[i];
				if (!character2.isDead && character2.isNormalCharacter && character2.race.IsSapient())
				{
					list.Add(character2);
				}
			}
			UIManager.Instance.ShowClickableObjectPicker(list, delegate(object o)
			{
				OnChooseCharacter(o, character);
			}, null, (Character t) => CanBeEvangelized(character, t), "", delegate(Character t)
			{
				OnHoverEnter(character, t);
			}, OnHoverExit, "", showCover: true, 25);
			RuinarchListPool<Character>.Release(list);
		}
		else
		{
			List<Character> list2 = RuinarchListPool<Character>.Claim();
			character.PopulateListOfCultistTargets(list2, (Character x) => !x.isDead && x.isNormalCharacter && x.race.IsSapient());
			UIManager.Instance.ShowClickableObjectPicker(list2, delegate(object o)
			{
				OnChooseCharacter(o, character);
			}, null, (Character t) => CanBeEvangelized(character, t), "", delegate(Character t)
			{
				OnHoverEnter(character, t);
			}, OnHoverExit, "", showCover: true, 25);
			RuinarchListPool<Character>.Release(list2);
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

	private bool CanBeEvangelized(Character owner, Character target)
	{
		if (target.traitContainer.HasTrait("Demon Cultist"))
		{
			return false;
		}
		switch (owner.relationshipContainer.GetAwarenessState(owner, target))
		{
		case AWARENESS_STATE.Missing:
			return false;
		case AWARENESS_STATE.Presumed_Dead:
			return false;
		default:
			if (target.traitContainer.HasTrait("Travelling"))
			{
				return false;
			}
			if (target.traitContainer.HasTrait("Restrained") && target.gridTileLocation.structure is TortureChambers)
			{
				return false;
			}
			if (owner.faction != null && target.faction != null && owner.faction.IsHostileWith(target.faction))
			{
				return false;
			}
			return true;
		}
	}

	private void OnHoverEnter(Character owner, Character target)
	{
		if (target.traitContainer.HasTrait("Demon Cultist"))
		{
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Schemes_Table", "Cannot_Target_Cultists")));
			return;
		}
		switch (owner.relationshipContainer.GetAwarenessState(owner, target))
		{
		case AWARENESS_STATE.Missing:
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Schemes_Table", "Cannot_Target_Missing")));
			return;
		case AWARENESS_STATE.Presumed_Dead:
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Schemes_Table", "Cannot_Target_Presumed_Dead")));
			return;
		}
		if (target.traitContainer.HasTrait("Travelling"))
		{
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Schemes_Table", "Cannot_Target_Travelling")));
			return;
		}
		if (target.traitContainer.HasTrait("Restrained") && target.gridTileLocation.structure is TortureChambers)
		{
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Schemes_Table", "Cannot_Target_Imprisoned")));
			return;
		}
		if (owner.faction != null && target.faction != null && owner.faction.IsHostileWith(target.faction))
		{
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(target.name, Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Schemes_Table", "Cannot_Target_Hostile")));
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
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "instructed_evangelize", LOG_TAG.Crimes, LOG_TAG.Player);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			if (!actor.jobComponent.TryCreateEvangelizeJob(character, JOB_TYPE.CULTIST_INSTRUCTION))
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "evangelize_fail", LOG_TAG.Player);
				log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
			}
			else
			{
				base.ActivateAbility((IPointOfInterest)actor);
			}
		}
	}
}
