using System.Collections.Generic;
using Traits;

public class RemoveFlawData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REMOVE_FLAW;

	public override string name => "Remove Flaw";

	public override string description => "This Action can be used to remove one negative Trait from a character.";

	public RemoveFlawData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			if (character.isDead)
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

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character character)
		{
			p_contextMenuItems.Clear();
			for (int i = 0; i < character.traitContainer.traits.Count; i++)
			{
				Trait trait = character.traitContainer.traits[i];
				if (!trait.isHidden && trait.type == TRAIT_TYPE.FLAW)
				{
					p_contextMenuItems.Add(trait);
				}
			}
			return p_contextMenuItems;
		}
		return null;
	}

	public void ActivateRemoveFlaw(string traitName, Character p_character)
	{
		if (RollSuccessChance(p_character) || p_character.traitContainer.HasTrait("Demon Cultist"))
		{
			Trait traitOrStatus = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
			p_character.traitContainer.RemoveTrait(p_character, traitName);
			Activate(p_character, bypassChance: true);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", name + " activated", LOG_TAG.Player);
			log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, traitOrStatus.localizedName, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
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
}
