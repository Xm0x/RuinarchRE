using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class SchemeData : PlayerAction
{
	public static bool alwaysSuccessScheme;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SCHEME;

	public override string name => "Scheme";

	public override string description => "This Action can be used to start various different schemes to manipulate world events.";

	public virtual string verbName => name;

	public override string localizedName => LocalizationManager.Instance.GetLocalizedValue("Schemes_Table", name);

	public override string localizedDescription => LocalizationManager.Instance.GetLocalizedValue("Schemes_Table", name + "_Description") ?? "";

	public SchemeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	protected virtual void OnSuccessScheme(Character character, object target)
	{
		LogSchemeCharacter(character, isSuccessful: true);
	}

	protected virtual void OnFailScheme(Character character, object target)
	{
		LogSchemeCharacter(character, isSuccessful: false);
	}

	protected virtual void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful)
	{
		if (isSuccessful)
		{
			ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "If_You_Say_So"), character, DialogItem.Position.Left);
			conversationList.Add(item);
		}
		else
		{
			ConversationData item2 = ObjectPoolManager.Instance.CreateNewConversationData(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "You_Cant_Make_Me"), character, DialogItem.Position.Left);
			conversationList.Add(item2);
		}
	}

	public virtual void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, object p_otherTarget, ref float p_newSuccessRate)
	{
	}

	public virtual string GetSuccessRateMultiplierText(Character p_targetCharacter, object p_otherTarget)
	{
		return string.Empty;
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter.faction != null && targetCharacter.faction.isPlayerFaction)
			{
				return false;
			}
			if (!targetCharacter.isDead)
			{
				return targetCharacter.limiterComponent.canPerform;
			}
			return false;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.faction != null && targetCharacter.faction.isPlayerFaction)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Target_Demon_Faction") + "|";
		}
		if (targetCharacter.isDead)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Target_Dead") + "|";
		}
		if (!targetCharacter.limiterComponent.canPerform)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Target_Cannot_Perform") + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		BaseSettlement baseSettlement = null;
		if (target is LocationStructure locationStructure)
		{
			baseSettlement = locationStructure.settlementLocation;
		}
		else if (target is BaseSettlement baseSettlement2)
		{
			baseSettlement = baseSettlement2;
		}
		if (target is Character character)
		{
			if (!character.isNormalCharacter && !character.isConsideredRatman)
			{
				return false;
			}
		}
		else if (baseSettlement != null && (baseSettlement.locationType != LOCATION_TYPE.VILLAGE || !(baseSettlement is NPCSettlement)))
		{
			return false;
		}
		if (base.IsValid(target) && PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSettlement != null)
		{
			return PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER);
		}
		return false;
	}

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (type == PLAYER_SKILL_TYPE.SCHEME && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null)
		{
			p_contextMenuItems.Clear();
			List<PLAYER_SKILL_TYPE> schemes = PlayerManager.Instance.player.playerSkillComponent.schemes;
			for (int i = 0; i < schemes.Count; i++)
			{
				PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = schemes[i];
				if (PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE) is PlayerAction playerAction && playerAction.IsValid(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget))
				{
					p_contextMenuItems.Add(playerAction);
				}
			}
			return p_contextMenuItems;
		}
		return null;
	}

	public bool ShouldSchemeBeSuccessful(Character character, object target, float successRate)
	{
		if (!alwaysSuccessScheme)
		{
			return GameUtilities.RollChance(successRate);
		}
		return true;
	}

	public void ProcessScheme(Character character, object target, bool isSuccessful)
	{
		List<ConversationData> list = RuinarchListPool<ConversationData>.Claim(3);
		PopulateSchemeConversation(list, character, target, isSuccessful);
		ShowSchemeConversation(list, localizedName);
		for (int i = 0; i < list.Count; i++)
		{
			ObjectPoolManager.Instance.ReturnConversationDataToPool(list[i]);
		}
		RuinarchListPool<ConversationData>.Release(list);
		if (isSuccessful)
		{
			OnSuccessScheme(character, target);
		}
		else
		{
			OnFailScheme(character, target);
		}
		PlayerAction playerActionData = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME);
		playerActionData.OnExecutePlayerSkill();
		OnExecutePlayerSkill();
		if (character.traitContainer.HasTrait("Grounded"))
		{
			character.traitContainer.GetTraitOrStatus<Grounded>("Grounded").DrainManaFromGrounded(playerActionData);
		}
		if (character.traitContainer.HasTrait("Nullchild"))
		{
			character.traitContainer.GetTraitOrStatus<Nullchild>("Nullchild").TryLockSkillUsedOnCharacter(playerActionData);
		}
	}

	private void ShowSchemeConversation(List<ConversationData> conversationList, string titleText)
	{
		UIManager.Instance.OpenConversationMenu(conversationList, titleText);
	}

	private void LogSchemeCharacter(Character p_targetCharacter, bool isSuccessful)
	{
		string key = "success_scheme_character";
		if (!isSuccessful)
		{
			key = "fail_scheme_character";
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", key, LOG_TAG.Player);
		log.AddToFillers(p_targetCharacter, p_targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, Utilities.GetArticleForWord(localizedName), LOG_IDENTIFIER.STRING_1);
		log.AddToFillers(null, localizedName, LOG_IDENTIFIER.STRING_2);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	public override bool IsInCooldown()
	{
		if (type == PLAYER_SKILL_TYPE.SCHEME && PlayerManager.Instance.player.playerSkillComponent.schemeInCooldown != PLAYER_SKILL_TYPE.NONE)
		{
			return true;
		}
		return base.IsInCooldown();
	}

	public override float GetCoverFillAmount()
	{
		if (type == PLAYER_SKILL_TYPE.SCHEME && PlayerManager.Instance.player.playerSkillComponent.schemeInCooldown != PLAYER_SKILL_TYPE.NONE && PlayerSkillManager.Instance.GetSkillData(PlayerManager.Instance.player.playerSkillComponent.schemeInCooldown) is PlayerAction { isInCooldown: not false } playerAction)
		{
			return 1f - (float)playerAction.currentCooldownTick / (float)playerAction.cooldown;
		}
		return base.GetCoverFillAmount();
	}

	public override int GetCurrentRemainingCooldownTicks()
	{
		if (type == PLAYER_SKILL_TYPE.SCHEME && PlayerManager.Instance.player.playerSkillComponent.schemeInCooldown != PLAYER_SKILL_TYPE.NONE && PlayerSkillManager.Instance.GetSkillData(PlayerManager.Instance.player.playerSkillComponent.schemeInCooldown) is PlayerAction { isInCooldown: not false } playerAction)
		{
			return playerAction.cooldown - playerAction.currentCooldownTick;
		}
		return base.GetCurrentRemainingCooldownTicks();
	}
}
