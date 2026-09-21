using System.Collections.Generic;
using Maccima_Games.Util;
using Traits;

public class TriggerFlawData : PlayerAction
{
	private readonly List<string> _triggerFlawPool;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TRIGGER_FLAW;

	public override string name => "Trigger Flaw";

	public override string description => "This Ability can be used to immediately activate an effect of a Villager's negative Trait. You may choose from the Villager's list of flaws that can be triggered.\nActivating Trigger Flaw produces a Chaos Orb. If the Villager successfully performs a task related to the Flaw, it will produce additional 2 Chaos Orbs.";

	public TriggerFlawData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
		_triggerFlawPool = new List<string>();
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (!targetCharacter.limiterComponent.canPerform)
			{
				return false;
			}
			return !targetCharacter.isDead;
		}
		return false;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			if (!character.isNormalCharacter || character.isConsideredRatman)
			{
				return false;
			}
			if (!character.traitContainer.HasTraitOf(TRAIT_TYPE.FLAW))
			{
				return false;
			}
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
		return text;
	}

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character character)
		{
			p_contextMenuItems.Clear();
			for (int i = 0; i < character.traitContainer.traits.Count; i++)
			{
				Trait trait = character.traitContainer.traits[i];
				if (trait.type == TRAIT_TYPE.FLAW)
				{
					p_contextMenuItems.Add(trait);
				}
			}
			return p_contextMenuItems;
		}
		return null;
	}

	public static void ActivateTriggerFlaw(Trait trait, Character p_character)
	{
		UIManager.Instance.HideObjectPicker();
		string text = trait.TriggerFlaw(p_character);
		if (text == "flaw_effect")
		{
			if (p_character.partyComponent.hasParty)
			{
				p_character.partyComponent.currentParty.RemoveMemberThatJoinedQuest(p_character);
			}
			Messenger.Broadcast(PlayerSkillSignals.FLAW_TRIGGER_SUCCESS, p_character);
			PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).OnExecutePlayerSkill();
			PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_TRIGGER_FLAW);
		}
		else
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Flaw_Fail_Default");
			string key = trait.name + " " + text;
			if (LocalizationManager.Instance.HasLocalizedValue("TraitTriggerFlaw_Table", key))
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("source", p_character.uiString);
				localizedValue = LocalizationManager.Instance.GetLocalizedValue("TraitTriggerFlaw_Table", key, dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
			}
			PlayerUI.Instance.ShowGeneralConfirmation(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Flaw_Failed"), localizedValue);
		}
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TRIGGER_FLAW);
		if (p_character.traitContainer.HasTrait("Grounded"))
		{
			p_character.traitContainer.GetTraitOrStatus<Grounded>("Grounded").DrainManaFromGrounded(skillData);
		}
		if (p_character.traitContainer.HasTrait("Nullchild"))
		{
			p_character.traitContainer.GetTraitOrStatus<Nullchild>("Nullchild").TryLockSkillUsedOnCharacter(skillData);
		}
		Messenger.Broadcast(PlayerSkillSignals.FLAW_TRIGGERED_BY_PLAYER, trait);
	}
}
