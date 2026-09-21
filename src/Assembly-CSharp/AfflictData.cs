using System.Collections.Generic;
using Object_Pools;
using Traits;
using UnityEngine.Localization.Settings;

public class AfflictData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.AFFLICT;

	public override string name => "Afflict";

	public override string description => "Afflict a Villager with a negative Trait.";

	public override string localizedName => LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", name);

	public override string localizedDescription => LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", name + "_Description") ?? "";

	public virtual string afflictionTraitName => string.Empty;

	public AfflictData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter.traitContainer.HasTrait(afflictionTraitName))
			{
				return false;
			}
			if (!targetCharacter.isDead)
			{
				return targetCharacter.limiterComponent.canBeAfflicted;
			}
			return false;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.traitContainer.HasTrait(afflictionTraitName))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Has_Flaw", targetCharacter) + "|";
		}
		if (!targetCharacter.limiterComponent.canBeAfflicted)
		{
			text = ((!targetCharacter.equipmentComponent.HasEquipment(TILE_OBJECT_TYPE.MANTRA)) ? (text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Immune_Affliction", targetCharacter) + "|") : (text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Mantra_Protect_Affliction", targetCharacter) + "|"));
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (!PlayerManager.Instance.player.playerSkillComponent.HasAfflictions())
		{
			return false;
		}
		if (target is Character character && (!character.isNormalCharacter || character.isConsideredRatman))
		{
			return false;
		}
		return base.IsValid(target);
	}

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (type == PLAYER_SKILL_TYPE.AFFLICT && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null)
		{
			p_contextMenuItems.Clear();
			List<PLAYER_SKILL_TYPE> afflictions = PlayerManager.Instance.player.playerSkillComponent.afflictions;
			for (int i = 0; i < afflictions.Count; i++)
			{
				PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = afflictions[i];
				if (PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE) is PlayerAction playerAction && playerAction.IsValid(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget))
				{
					p_contextMenuItems.Add(playerAction);
				}
			}
			return p_contextMenuItems;
		}
		return null;
	}

	public virtual void ApplyAfflictionEffects(IPointOfInterest target, int overridenDuration = 0)
	{
		if (overridenDuration > 0)
		{
			target.traitContainer.AddTrait(target, afflictionTraitName, null, bypassElementalChance: false, overridenDuration);
		}
		else
		{
			target.traitContainer.AddTrait(target, afflictionTraitName);
		}
	}

	protected void PlayerApplyAfflictionToTarget(IPointOfInterest target, int overridenDuration = 0)
	{
		LogAfflictionAndAddAfflictionAsAppliedByPlayer(logName: (!(LocalizationSettings.SelectedLocale.Identifier.Code == "tr-TR")) ? TraitManager.Instance.GetLocalizedNameOfTrait(afflictionTraitName) : localizedName, traitName: afflictionTraitName, target: target);
		ApplyAfflictionEffects(target, overridenDuration);
		if (target is Character)
		{
			if (target.traitContainer.HasTrait("Grounded"))
			{
				target.traitContainer.GetTraitOrStatus<Grounded>("Grounded").DrainManaFromGrounded(this);
			}
			if (target.traitContainer.HasTrait("Nullchild"))
			{
				target.traitContainer.GetTraitOrStatus<Nullchild>("Nullchild").TryLockSkillUsedOnCharacter(this);
			}
		}
	}

	private void LogAfflictionAndAddAfflictionAsAppliedByPlayer(string traitName, IPointOfInterest target, string logName)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "player_afflicted", LOG_TAG.Player, LOG_TAG.Life_Changes);
		log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, logName, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
		LogPool.Release(log);
		if (target is Character character)
		{
			character.AddAfflictionByPlayer(traitName);
		}
	}
}
