using UtilityScripts;

public class TurnToStoneSpecialSkill : CombatSpecialSkill
{
	public TurnToStoneSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Turn_To_Stone, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Magical, 20, 2)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		Character validTargetFor = GetValidTargetFor(p_character);
		if (validTargetFor != null && !validTargetFor.traitContainer.HasTrait("Petrasol", "Stoned"))
		{
			int p_value = 25;
			if (!validTargetFor.limiterComponent.canPerform)
			{
				p_value = 50;
			}
			validTargetFor.piercingAndResistancesComponent.ModifyValueByResistance(ref p_value, ELEMENTAL_TYPE.Earth, p_character.piercingAndResistancesComponent.piercingPower);
			string log = string.Empty;
			if (GameUtilities.RollChance(p_value, ref log))
			{
				validTargetFor.traitContainer.AddTrait(validTargetFor, "Stoned");
				if (validTargetFor.hasMarker)
				{
					AkSoundEngine.PostEvent("Play_Turn_To_Stone", validTargetFor.marker.gameObject);
				}
				p_character.combatComponent.RemoveHostileInRange(validTargetFor);
				p_character.combatComponent.RemoveAvoidInRange(validTargetFor);
			}
			return true;
		}
		return base.TryActivateSkill(p_character);
	}

	protected override Character GetValidTargetFor(Character p_character)
	{
		if (p_character.hasMarker)
		{
			if (p_character.combatComponent.isInCombat && p_character.stateComponent.currentState is CombatState { currentClosestHostile: Character { isDead: false } currentClosestHostile })
			{
				return currentClosestHostile;
			}
			for (int i = 0; i < p_character.combatComponent.hostilesInRange.Count; i++)
			{
				if (p_character.combatComponent.hostilesInRange[i] is Character { isDead: false } character && !character.traitContainer.HasTrait("Petrasol", "Stoned"))
				{
					return character;
				}
			}
		}
		return base.GetValidTargetFor(p_character);
	}
}
