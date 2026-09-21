using System.Collections.Generic;
using Inner_Maps;

public class LeaveVillageData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LEAVE_VILLAGE;

	public override string name => "Leave Village";

	public override string description => "Convince a Villager to leave their current Village.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public LeaveVillageData()
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
			if (targetCharacter.homeSettlement == null || targetCharacter.isConsideredRatman)
			{
				return false;
			}
			if (targetCharacter.homeSettlement.locationType != LOCATION_TYPE.VILLAGE)
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
		if (targetCharacter.homeSettlement == null || targetCharacter.homeSettlement.locationType != LOCATION_TYPE.VILLAGE)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Leave_Village_No_Village") + "|";
		}
		if (targetCharacter.isConsideredRatman)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Leave_Village_Ratmen") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Village, character);
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

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character targetCharacter, object target, bool isSuccessful)
	{
		ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Leave_Village_Message"), null, DialogItem.Position.Right);
		conversationList.Add(item);
		base.PopulateSchemeConversation(conversationList, targetCharacter, target, isSuccessful);
	}
}
