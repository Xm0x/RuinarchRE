using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class LeaveHomeData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LEAVE_HOME;

	public override string name => "Leave Home";

	public override string description => "Convince a Villager to leave their current Home.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public LeaveHomeData()
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
			if (targetCharacter.homeStructure == null || targetCharacter.isConsideredRatman)
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
		if (targetCharacter.homeStructure == null)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Homeless") + "|";
		}
		if (targetCharacter.isConsideredRatman)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Leave_Home_Ratmen") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Home, character);
		BaseSettlement homeSettlement = character.homeSettlement;
		if (homeSettlement == null)
		{
			return;
		}
		LocationStructure randomStructureThatCharacterHasPathTo = homeSettlement.GetRandomStructureThatCharacterHasPathTo(character, character.previousCharacterDataComponent.previousHomeStructure, character.homeStructure);
		if (randomStructureThatCharacterHasPathTo != null)
		{
			LocationGridTile randomPassableTile = randomStructureThatCharacterHasPathTo.GetRandomPassableTile();
			if (randomPassableTile != null)
			{
				character.jobComponent.CreateGoToJob(randomPassableTile);
			}
		}
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character targetCharacter, object target, bool isSuccessful)
	{
		ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Leave_Home_Message"), null, DialogItem.Position.Right);
		conversationList.Add(item);
		base.PopulateSchemeConversation(conversationList, targetCharacter, target, isSuccessful);
	}

	public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, object p_otherTarget, ref float p_newSuccessRate)
	{
		p_newSuccessRate *= 2f;
	}

	public override string GetSuccessRateMultiplierText(Character p_targetCharacter, object p_otherTarget)
	{
		return "Leave Home: <color=white>x2</color>";
	}
}
