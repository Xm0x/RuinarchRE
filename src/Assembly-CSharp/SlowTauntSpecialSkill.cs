using System.Collections.Generic;
using UtilityScripts;

public class SlowTauntSpecialSkill : CombatSpecialSkill
{
	public SlowTauntSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Slow_Taunt, COMBAT_SPECIAL_SKILL_TARGET.Multiple, COMBAT_SPECIAL_SKILL_CATEGORY.Tanking, 10, 1)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		bool result = false;
		List<Character> list = RuinarchListPool<Character>.Claim();
		PopulateValidTargetsFor(p_character, list);
		if (list.Count > 0)
		{
			result = true;
			GameManager.Instance.CreateParticleEffectAt(p_character, PARTICLE_EFFECT.Taunt, allowRotation: false);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].interruptComponent.TriggerInterrupt(INTERRUPT.Taunted, p_character);
			}
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	protected override void PopulateValidTargetsFor(Character p_character, List<Character> p_validTargets)
	{
		if (!p_character.hasMarker)
		{
			return;
		}
		for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++)
		{
			Character character = p_character.marker.inVisionCharacters[i];
			if (!character.isDead && !character.traitContainer.HasTrait("Taunted") && p_character.faction != null && character.faction != null && p_character.faction.IsHostileWith(character.faction) && !p_character.combatComponent.IsAvoidInRange(character))
			{
				if (character.combatComponent.IsCurrentlyAttackingFriendlyWith(p_character))
				{
					p_validTargets.Add(character);
				}
				else if (p_character.faction != null && p_character.faction.isPlayerFaction && character.combatComponent.IsCurrentlyAttackingDemonicStructure())
				{
					p_validTargets.Add(character);
				}
			}
		}
	}
}
