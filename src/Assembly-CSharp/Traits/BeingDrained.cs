using UnityEngine;

namespace Traits;

public class BeingDrained : Status
{
	public BeingDrained()
	{
		name = "Being Drained";
		description = "This character is being drained!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		isHidden = true;
		hindersSocials = true;
		hindersPerform = true;
		hindersWitness = true;
		AddTraitOverrideFunctionIdentifier("Tick_Ended_Trait");
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override void OnTickEnded(ITraitable traitable)
	{
		base.OnTickEnded(traitable);
		if (traitable is Character p_character)
		{
			DrainPerTick(p_character);
		}
	}

	private void DrainPerTick(Character p_character)
	{
		if (!p_character.isDead)
		{
			int num = 0;
			num = ((p_character.currentStructure == null || p_character.currentStructure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS) ? Mathf.RoundToInt((float)p_character.maxHP * 0.5f) : Mathf.RoundToInt((float)p_character.maxHP * 0.25f));
			int num2 = ((p_character is Summon) ? 1 : 1);
			if (p_character.hasMarker && PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.DRAIN_SPIRIT).TryDecreaseRemainingChaosOrbs(num2))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, num2, p_character.gridTileLocation.parentMap);
			}
			p_character.AdjustHP(-num, ELEMENTAL_TYPE.Normal, triggerDeath: true, this, null, showHPBar: true, 0f, isPlayerSource: false, isTrueDamage: true);
			if (p_character.isDead && p_character.hasMarker && p_character.gridTileLocation != null)
			{
				GameManager.Instance.CreateParticleEffectAt(p_character.gridTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
				p_character.DestroyMarker();
			}
		}
	}
}
