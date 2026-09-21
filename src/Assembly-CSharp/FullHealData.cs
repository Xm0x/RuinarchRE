using Inner_Maps.Location_Structures;
using UnityEngine;

public class FullHealData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FULL_HEAL;

	public override string name => "Full Heal";

	public override string description => "This Action fully replenishes a character's HP.";

	public FullHealData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			GameObject in_gameObjectID = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Heal, allowRotation: false);
			AkSoundEngine.PostEvent("Play_Heal", in_gameObjectID);
			character.AdjustHP(character.maxHP, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true);
		}
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead)
		{
			return false;
		}
		if (targetCharacter.IsHealthFull())
		{
			return false;
		}
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.isDead)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Dead", targetCharacter) + "|";
		}
		if (targetCharacter.IsHealthFull())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Full_HP", targetCharacter) + "|";
		}
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Being_Drained_Cannot_Heal") + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (base.IsValid(target))
		{
			if (target is Character character)
			{
				if (!(character.currentStructure is Kennel))
				{
					return character.currentStructure is TortureChambers;
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
