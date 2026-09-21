using System.Collections.Generic;
using UtilityScripts;

public class GroupHealSpecialSkill : CombatSpecialSkill
{
	public GroupHealSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Group_Heal, COMBAT_SPECIAL_SKILL_TARGET.Multiple, COMBAT_SPECIAL_SKILL_CATEGORY.Healing, 20, 3)
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
				character.AdjustHP(100, ELEMENTAL_TYPE.Normal);
				GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Heal, allowRotation: false);
			}
			p_character.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(3, p_character);
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
		if (!p_character.isDead && !p_character.IsHealthFull())
		{
			p_validTargets.Add(p_character);
		}
		for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++)
		{
			Character character = p_character.marker.inVisionCharacters[i];
			if (!character.isDead && !character.IsHealthFull() && p_character.faction != null && character.faction != null && p_character.faction.IsFriendlyWith(character.faction) && !p_character.combatComponent.IsHostileInRange(character) && !p_character.combatComponent.IsAvoidInRange(character))
			{
				p_validTargets.Add(character);
			}
		}
	}
}
