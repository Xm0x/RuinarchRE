using System.Collections.Generic;

public class ResignData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RESIGN;

	public override string name => "Resign";

	public override string description => "Force a Faction Leader or Settlement Ruler to resign.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public ResignData()
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
			Faction faction = targetCharacter.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Undead)
			{
				return false;
			}
			if (!targetCharacter.isFactionLeader && !targetCharacter.isSettlementRuler)
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
		Faction faction = targetCharacter.faction;
		if (faction != null && faction.factionType.type == FACTION_TYPE.Undead)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Resign_Undead") + "|";
		}
		if (!targetCharacter.isFactionLeader && !targetCharacter.isSettlementRuler)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Resign_Not_Leader") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		character.interruptComponent.TriggerInterrupt(INTERRUPT.Resign, character);
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character targetCharacter, object target, bool isSuccessful)
	{
		ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Resign_Message"), null, DialogItem.Position.Right);
		conversationList.Add(item);
		base.PopulateSchemeConversation(conversationList, targetCharacter, target, isSuccessful);
	}
}
