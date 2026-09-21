using System.Collections.Generic;
using UtilityScripts;

public class ThornsSpecialSkill : CombatSpecialSkill
{
	public ThornsSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Thorns, COMBAT_SPECIAL_SKILL_TARGET.Multiple, COMBAT_SPECIAL_SKILL_CATEGORY.Magical, 20, 1)
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
				character.traitContainer.AddTrait(character, "Thorns");
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
		if (!p_character.traitContainer.HasTrait("Thorns"))
		{
			p_validTargets.Add(p_character);
		}
		for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++)
		{
			Character character = p_character.marker.inVisionCharacters[i];
			if (!character.isDead && !character.traitContainer.HasTrait("Thorns") && p_character.partyComponent.hasParty && p_character.partyComponent.currentParty.IsMember(character))
			{
				p_validTargets.Add(character);
			}
		}
	}
}
