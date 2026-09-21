using System.Collections.Generic;
using UtilityScripts;

public class StoneskinSpecialSkill : CombatSpecialSkill
{
	public StoneskinSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Stoneskin, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Magical, 10, 1)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		Character validTargetFor = GetValidTargetFor(p_character);
		if (validTargetFor != null)
		{
			validTargetFor.traitContainer.AddTrait(validTargetFor, "Stoneskin");
			return true;
		}
		return base.TryActivateSkill(p_character);
	}

	protected override Character GetValidTargetFor(Character p_character)
	{
		if (p_character.hasMarker)
		{
			Character result = null;
			List<Character> list = RuinarchListPool<Character>.Claim();
			if (!p_character.traitContainer.HasTrait("Stoneskin"))
			{
				list.Add(p_character);
			}
			for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++)
			{
				Character character = p_character.marker.inVisionCharacters[i];
				if (!character.isDead && !character.traitContainer.HasTrait("Stoneskin") && p_character.partyComponent.hasParty && p_character.partyComponent.currentParty.IsMember(character))
				{
					list.Add(character);
				}
			}
			if (list.Count > 0)
			{
				result = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<Character>.Release(list);
			return result;
		}
		return null;
	}
}
