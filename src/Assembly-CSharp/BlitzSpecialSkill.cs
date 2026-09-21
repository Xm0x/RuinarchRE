using System.Collections.Generic;
using UtilityScripts;

public class BlitzSpecialSkill : CombatSpecialSkill
{
	public BlitzSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Blitz, COMBAT_SPECIAL_SKILL_TARGET.Multiple, COMBAT_SPECIAL_SKILL_CATEGORY.Physical, 50, 1)
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
				character.traitContainer.AddTrait(character, "Blitz Buff", p_character);
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
		if (!p_character.isDead && !p_character.traitContainer.HasTrait("Blitz Buff"))
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
			if (!character.isDead && !character.traitContainer.HasTrait("Blitz Buff") && p_character.partyComponent.currentParty.IsMember(character))
			{
				p_validTargets.Add(character);
			}
		}
	}
}
