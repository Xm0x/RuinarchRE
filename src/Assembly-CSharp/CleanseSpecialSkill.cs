using System.Collections.Generic;
using UtilityScripts;

public class CleanseSpecialSkill : CombatSpecialSkill
{
	public CleanseSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Cleanse, COMBAT_SPECIAL_SKILL_TARGET.Multiple, COMBAT_SPECIAL_SKILL_CATEGORY.Physical, 50, 1)
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
				character.traitContainer.RemoveStatusAndStacks(character, "Poisoned", p_character);
				character.traitContainer.RemoveStatusAndStacks(character, "Zapped", p_character);
				character.traitContainer.RemoveStatusAndStacks(character, "Freezing", p_character);
				character.traitContainer.RemoveStatusAndStacks(character, "Burning", p_character);
				character.traitContainer.RemoveStatusAndStacks(character, "Frozen", p_character);
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
		if (!p_character.isDead && (p_character.traitContainer.HasTrait("Poisoned") || p_character.traitContainer.HasTrait("Zapped") || p_character.traitContainer.HasTrait("Freezing") || p_character.traitContainer.HasTrait("Burning") || p_character.traitContainer.HasTrait("Frozen")))
		{
			p_validTargets.Add(p_character);
		}
		if (!p_character.partyComponent.hasParty)
		{
			return;
		}
		for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++)
		{
			Character character = p_character.marker.inVisionCharacters[i];
			if (!character.isDead && p_character.partyComponent.currentParty.IsMember(character) && (character.traitContainer.HasTrait("Poisoned") || character.traitContainer.HasTrait("Zapped") || character.traitContainer.HasTrait("Freezing") || character.traitContainer.HasTrait("Burning") || character.traitContainer.HasTrait("Frozen")))
			{
				p_validTargets.Add(character);
			}
		}
	}
}
