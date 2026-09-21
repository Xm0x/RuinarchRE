using System.Collections.Generic;
using Inner_Maps;

public class LeaveFactionData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LEAVE_FACTION;

	public override string name => "Leave Faction";

	public override string description => "Convince a Villager to leave their current Faction.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public LeaveFactionData()
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
			bool num = targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer;
			bool isConsideredRatman = targetCharacter.isConsideredRatman;
			if (!num && !isConsideredRatman)
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
		bool num = targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer;
		bool isConsideredRatman = targetCharacter.isConsideredRatman;
		if (!num)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Not_In_Major_Faction") + "|";
		}
		if (targetCharacter.race == RACE.RATMAN && !isConsideredRatman)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Leave_Faction_Not_Ratman") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
		Area randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage = character.currentRegion.GetRandomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage(character);
		if (randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage != null)
		{
			LocationGridTile randomPassableTile = randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage.gridTileComponent.GetRandomPassableTile();
			if (randomPassableTile != null)
			{
				character.jobComponent.CreateGoToJob(randomPassableTile);
			}
		}
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful)
	{
		ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Leave_Faction_Message"), null, DialogItem.Position.Right);
		conversationList.Add(item);
		base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
	}

	public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, object p_otherTarget, ref float p_newSuccessRate)
	{
		if (p_targetCharacter.traitContainer.HasTrait("Treacherous"))
		{
			p_newSuccessRate *= 2f;
		}
		if (p_targetCharacter.isFactionLeader || p_targetCharacter.isSettlementRuler)
		{
			p_newSuccessRate *= 0.25f;
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
		if (p_targetCharacter.isFactionLeader)
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			text = text + p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Faction Leader") + ": <color=white>x0.25</color>";
		}
		else if (p_targetCharacter.isSettlementRuler)
		{
			if (text != string.Empty)
			{
				text += "\n";
			}
			text = text + p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Settlement Ruler") + ": <color=white>x0.25</color>";
		}
		if (text != string.Empty)
		{
			return text;
		}
		return base.GetSuccessRateMultiplierText(p_targetCharacter, p_otherTarget);
	}
}
