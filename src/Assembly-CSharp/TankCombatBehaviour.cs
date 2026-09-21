public class TankCombatBehaviour : CharacterCombatBehaviour
{
	public TankCombatBehaviour()
		: base(CHARACTER_COMBAT_BEHAVIOUR.Tank)
	{
	}

	public override bool DetermineCombatBehaviour(Character p_character, CombatState p_combatState)
	{
		if (p_character.combatComponent.combatBehaviourParent.canDoTankBehaviour && p_character.partyComponent.isMemberThatJoinedQuest && p_character.hasMarker)
		{
			Character character = null;
			int num = 0;
			for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++)
			{
				Character character2 = p_character.marker.inVisionCharacters[i];
				if (!character2.isDead && character2.combatComponent.IsCurrentlyAttackingPartyMateOf(p_character) && (character == null || character2.currentHP > num))
				{
					character = character2;
					num = character2.currentHP;
				}
			}
			if (character != null)
			{
				if (p_character.combatComponent.IsHostileInRange(character))
				{
					p_combatState.SetForcedTarget(character);
					p_character.combatComponent.combatBehaviourParent.SetCanDoTankBehaviour(p_state: false);
					return true;
				}
				if (p_character.combatComponent.Fight(character, "Tanking"))
				{
					p_combatState.SetForcedTarget(character);
					p_character.combatComponent.combatBehaviourParent.SetCanDoTankBehaviour(p_state: false);
					return true;
				}
			}
		}
		return base.DetermineCombatBehaviour(p_character, p_combatState);
	}
}
