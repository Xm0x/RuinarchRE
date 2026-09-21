using System.Collections.Generic;
using Object_Pools;

public class OverthrowLeaderData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.OVERTHROW_LEADER;

	public override string name => "Overthrow Leader";

	public override string description => "Convince a Successor to depose the current Faction Leader.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public OverthrowLeaderData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character p_targetCharacter)
		{
			UIManager.Instance.ShowSchemeUI(p_targetCharacter, null, this);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (targetCharacter.faction == null || targetCharacter.faction.leader == null || !(targetCharacter.faction.leader is Character))
			{
				return false;
			}
			if (!targetCharacter.faction.successionComponent.IsSuccessor(targetCharacter))
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
		if (targetCharacter.faction == null || targetCharacter.faction.leader == null || !(targetCharacter.faction.leader is Character))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Overthrow_No_Faction_Or_Leader") + "|";
		}
		if (!targetCharacter.faction.successionComponent.IsSuccessor(targetCharacter))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Overthrow_Not_Successor") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		Character character2 = character.faction.leader as Character;
		character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, character, "succession");
		if (character2 != null)
		{
			if (character2.isSettlementRuler)
			{
				character2.homeSettlement.SetRuler(null);
			}
			if (!character2.relationshipContainer.HasGrudgeAgainst(character))
			{
				character2.relationshipContainer.SetHasGrudgeAgainst(character2, character, p_state: true);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Relationships", "Relationships_Table", "Grudge_Overthrow_Leader", LOG_TAG.Life_Changes);
				log.AddToFillers(character2, character2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance?.player?.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
		}
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful)
	{
		if (character.faction != null && character.faction.leader != null)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Overthrow_Leader_Message");
			log.AddToFillers(character.faction.leader as Character, character.faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText = log.logText;
			LogPool.Release(log);
			ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(logText, null, DialogItem.Position.Right);
			conversationList.Add(item);
		}
		base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
	}

	public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, object p_otherTarget, ref float p_newSuccessRate)
	{
		if (p_targetCharacter.traitContainer.HasTrait("Treacherous"))
		{
			p_newSuccessRate *= 2f;
		}
		if (p_targetCharacter.traitContainer.HasTrait("Ambitious"))
		{
			p_newSuccessRate *= 2f;
		}
		base.ProcessSuccessRateWithMultipliers(p_targetCharacter, p_otherTarget, ref p_newSuccessRate);
	}

	public override string GetSuccessRateMultiplierText(Character p_targetCharacter, object p_otherTarget)
	{
		string text = string.Empty;
		if (p_targetCharacter.traitContainer.HasTrait("Treacherous"))
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			text = text + p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + TraitManager.Instance.GetLocalizedNameOfTrait("Treacherous") + ": <color=white>x2</color>";
		}
		if (p_targetCharacter.traitContainer.HasTrait("Ambitious"))
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			text = text + p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + TraitManager.Instance.GetLocalizedNameOfTrait("Ambitious") + ": <color=white>x2</color>";
		}
		if (text != string.Empty)
		{
			return text;
		}
		return base.GetSuccessRateMultiplierText(p_targetCharacter, p_otherTarget);
	}
}
