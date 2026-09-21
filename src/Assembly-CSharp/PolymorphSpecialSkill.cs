using System.Collections.Generic;
using UtilityScripts;

public class PolymorphSpecialSkill : CombatSpecialSkill
{
	public PolymorphSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Polymorph, COMBAT_SPECIAL_SKILL_TARGET.Multiple, COMBAT_SPECIAL_SKILL_CATEGORY.Magical, 50, 3)
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
			for (int i = 0; i < list.Count; i++)
			{
				Character character = list[i];
				character.traitContainer.AddTrait(character, "Polymorphed");
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
		for (int i = 0; i < p_character.combatComponent.hostilesInRange.Count; i++)
		{
			if (p_character.combatComponent.hostilesInRange[i] is Character { isDead: false } character && !character.traitContainer.HasTrait("Polymorphed"))
			{
				p_validTargets.Add(character);
			}
		}
	}
}
