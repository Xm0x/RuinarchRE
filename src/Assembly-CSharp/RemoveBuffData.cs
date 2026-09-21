using System.Collections.Generic;
using Traits;

public class RemoveBuffData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REMOVE_BUFF;

	public override string name => "Remove Buff";

	public override string description => "This Ability can be used to remove any positive Trait from a character.\nRemoving a buff from a hostile Villager will produce a Chaos Orb.";

	public override bool canBeCastOnBlessed => HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Can_Remove_Blessed_Trait);

	public RemoveBuffData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		base.ActivateAbility(targetPOI);
		Messenger.Broadcast(PlayerSkillSignals.REMOVE_BUFF_ACTIVATED, targetPOI);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			if (character.isDead)
			{
				return false;
			}
			if (!character.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF))
			{
				return false;
			}
		}
		return base.IsValid(target);
	}

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character character)
		{
			p_contextMenuItems.Clear();
			for (int i = 0; i < character.traitContainer.traits.Count; i++)
			{
				Trait trait = character.traitContainer.traits[i];
				if (!trait.isHidden && trait.type == TRAIT_TYPE.BUFF)
				{
					p_contextMenuItems.Add(trait);
				}
			}
			return p_contextMenuItems;
		}
		return null;
	}

	public void ActivateRemoveBuff(string traitName, Character p_character)
	{
		if (RollSuccessChance(p_character) || p_character.traitContainer.HasTrait("Demon Cultist"))
		{
			Trait traitOrStatus = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
			p_character.traitContainer.RemoveTrait(p_character, traitOrStatus);
			Activate(p_character, bypassChance: true);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", name + " activated", LOG_TAG.Player);
			log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, traitOrStatus.localizedName, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_REMOVE_BUFF);
		}
		else
		{
			OnExecutePlayerSkill();
			p_character.reactionComponent.ResistRuinarchPower();
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_RESISTED, (PlayerAction)this);
		}
		if (p_character.traitContainer.HasTrait("Grounded"))
		{
			p_character.traitContainer.GetTraitOrStatus<Grounded>("Grounded").DrainManaFromGrounded(this);
		}
		if (p_character.traitContainer.HasTrait("Nullchild"))
		{
			p_character.traitContainer.GetTraitOrStatus<Nullchild>("Nullchild").TryLockSkillUsedOnCharacter(this);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Can_Remove_Blessed_Trait))
		{
			if (targetCharacter != null)
			{
				if (targetCharacter.traitContainer.HasTrait("Dark Blessing"))
				{
					return false;
				}
			}
			return CanPerformAbility();
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		if (HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Can_Remove_Blessed_Trait))
		{
			if (targetCharacter != null)
			{
				if (targetCharacter.traitContainer.HasTrait("Dark Blessing"))
				{
					return GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Target_Dark_Blessing") + "|";
				}
				return base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
			}
			return string.Empty;
		}
		return base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
	}
}
