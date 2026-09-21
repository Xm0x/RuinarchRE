using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Object_Pools;
using UtilityScripts;

public class AssassinateData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ASSASSINATE;

	public override string name => "Assassinate";

	public override string description => "Instruct a character to assassinate a villager that he/she knows.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public AssassinateData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		Character character = targetPOI as Character;
		if (character != null)
		{
			List<Character> list = RuinarchListPool<Character>.Claim(50);
			PopulateListOfAssassinateTargets(character, list);
			UIManager.Instance.ShowClickableObjectPicker(list, delegate(object o)
			{
				OnChooseCharacter(o, character);
			}, null, (Character t) => CanBeAssassinated(character, t), "", delegate(Character t)
			{
				OnHoverEnter(character, t);
			}, OnHoverExit, "", showCover: true, 25);
			RuinarchListPool<Character>.Release(list);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (!targetCharacter.isNormalCharacter)
			{
				return false;
			}
			if (targetCharacter.jobQueue.HasJob(JOB_TYPE.ASSASSINATE))
			{
				return false;
			}
			return true;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!targetCharacter.isNormalCharacter)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Not_Villager") + "|";
		}
		if (targetCharacter.jobQueue.HasJob(JOB_TYPE.ASSASSINATE))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Assassinating") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		if (target is Character targetCharacter)
		{
			character.jobComponent.CreateAssassinateTargetJob(targetCharacter);
		}
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful)
	{
		if (target is Character character2)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Asssassinate_Message");
			log.AddToFillers(character2, character2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText = log.logText;
			LogPool.Release(log);
			ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(logText, null, DialogItem.Position.Right);
			conversationList.Add(item);
		}
		base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
	}

	public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, object p_otherTarget, ref float p_newSuccessRate)
	{
		if (p_otherTarget is Character target)
		{
			switch (p_targetCharacter.relationshipContainer.GetOpinionLabel(target))
			{
			case "Close Friend":
				p_newSuccessRate *= 0.5f;
				break;
			case "Friend":
				p_newSuccessRate *= 0.65f;
				break;
			case "Acquaintance":
				p_newSuccessRate *= 0.85f;
				break;
			}
		}
		base.ProcessSuccessRateWithMultipliers(p_targetCharacter, p_otherTarget, ref p_newSuccessRate);
	}

	public override string GetSuccessRateMultiplierText(Character p_targetCharacter, object p_otherTarget)
	{
		if (p_otherTarget is Character character)
		{
			string text = string.Empty;
			string opinionLabel = p_targetCharacter.relationshipContainer.GetOpinionLabel(character);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", opinionLabel);
			switch (opinionLabel)
			{
			case "Close Friend":
				if (text != string.Empty)
				{
					text += "\n";
				}
				text = text + character.visuals.GetCharacterNameWithIconAndColor() + " - " + localizedValue + ": <color=white>x0.5</color>";
				break;
			case "Friend":
				if (text != string.Empty)
				{
					text += "\n";
				}
				text = text + character.visuals.GetCharacterNameWithIconAndColor() + " - " + localizedValue + ": <color=white>x0.65</color>";
				break;
			case "Acquaintance":
				if (text != string.Empty)
				{
					text += "\n";
				}
				text = text + character.visuals.GetCharacterNameWithIconAndColor() + " - " + localizedValue + ": <color=white>x0.85</color>";
				break;
			}
			if (text != string.Empty)
			{
				return text;
			}
		}
		return base.GetSuccessRateMultiplierText(p_targetCharacter, p_otherTarget);
	}

	private void OnChooseCharacter(object obj, Character source)
	{
		if (obj is Character p_otherTarget)
		{
			UIManager.Instance.HideObjectPicker();
			UIManager.Instance.ShowSchemeUI(source, p_otherTarget, this);
		}
	}

	private void PopulateListOfAssassinateTargets(Character p_character, List<Character> p_choices)
	{
		for (int i = 0; i < p_character.relationshipContainer.charactersWithOpinion.Count; i++)
		{
			Character character = p_character.relationshipContainer.charactersWithOpinion[i];
			if (!character.isDead)
			{
				p_choices.Add(character);
			}
		}
	}

	private bool CanBeAssassinated(Character owner, Character target)
	{
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
}
