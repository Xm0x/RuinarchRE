using UnityEngine;

namespace Traits;

public class Thorns : Status
{
	public Thorns()
	{
		name = "Thorns";
		description = "Reflects some physical damage back at attacker.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = true;
		stackLimit = 1;
		ticksDuration = 20;
	}

	public void ReflectDamage(Character p_attacker, Character p_target, bool p_isAttackLethal)
	{
		p_attacker.AdjustHP(-10, ELEMENTAL_TYPE.Normal, p_isAttackLethal, p_target);
		if (p_attacker.HasHealth() || p_attacker.isDead)
		{
			return;
		}
		AgitateData obj = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.AGITATE) as AgitateData;
		bool flag = obj != null && obj.currentLevel >= 3 && p_attacker.traitContainer.HasTrait("Agitated");
		if (!p_attacker.traitContainer.HasTrait("Sturdy") && !flag)
		{
			if (p_attacker.traitContainer.HasTrait("Unconscious"))
			{
				int hP = Mathf.CeilToInt((float)p_attacker.maxHP * 0.1f);
				p_attacker.SetHP(hP);
			}
			else
			{
				p_attacker.traitContainer.AddTrait(p_attacker, "Unconscious", p_target);
			}
		}
	}
}
