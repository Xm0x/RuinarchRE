using Inner_Maps;
using UnityEngine;

public class WindSliceSpecialSkill : CombatSpecialSkill
{
	public WindSliceSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Wind_Slice, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Physical, 10, 2)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		if (p_character.stateComponent.currentState is CombatState { currentClosestHostile: not null } combatState && p_character.combatComponent.isInActualCombat)
		{
			int p_attackPower = Mathf.FloorToInt((float)p_character.combatComponent.attack * 1.5f);
			if (p_character.hasMarker)
			{
				InnerMapManager.Instance.ShowTextEffect("Wind slice!", Color.yellow, p_character.worldPosition);
			}
			string attackSummary = string.Empty;
			combatState.currentClosestHostile.OnHitByAttackFrom(p_character, combatState, p_attackPower, ELEMENTAL_TYPE.Wind, ref attackSummary);
			return true;
		}
		return false;
	}

	public override bool TryActivateSkill(Character p_character, Character rider)
	{
		if (rider.stateComponent.currentState is CombatState { currentClosestHostile: not null } combatState && rider.combatComponent.isInActualCombat)
		{
			int p_attackPower = Mathf.FloorToInt((float)p_character.combatComponent.attack * 1.5f);
			if (rider.hasMarker)
			{
				InnerMapManager.Instance.ShowTextEffect(p_character.name + " Wind slice!", Color.yellow, rider.worldPosition);
			}
			string attackSummary = string.Empty;
			combatState.currentClosestHostile.OnHitByAttackFrom(rider, combatState, p_attackPower, ELEMENTAL_TYPE.Wind, ref attackSummary);
			return true;
		}
		return false;
	}
}
