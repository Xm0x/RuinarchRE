using System.Collections.Generic;
using Traits;
using UnityEngine;
using UtilityScripts;

public class TerrifyingHowlCollisionListener : ParticleCollisionListener
{
	private HashSet<Character> affectedCharacters;

	private void OnEnable()
	{
		affectedCharacters = new HashSet<Character>();
	}

	private void OnDisable()
	{
		affectedCharacters = null;
	}

	protected override void OnParticleCollision(GameObject other)
	{
		if (!other.CompareTag("Character Marker"))
		{
			return;
		}
		Character character = other.GetComponent<CharacterMarker>().character;
		if (affectedCharacters.Contains(character) || (character.faction != null && character.faction.isPlayerFaction) || character.traitContainer.HasTrait("Hibernating") || character.traitContainer.HasTrait("Hidden"))
		{
			return;
		}
		int p_value = 100;
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.TERRIFYING_HOWL);
		RESISTANCE resistanceType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.TERRIFYING_HOWL).resistanceType;
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		float resistanceValue = character.piercingAndResistancesComponent.GetResistanceValue(resistanceType);
		CombatManager.ModifyValueByPiercingAndResistance(ref p_value, pierceBasedOnCurrentLevel, resistanceValue);
		if (GameUtilities.RollChance(p_value))
		{
			CombatManager.Instance.CreateHitEffectAt(character, ELEMENTAL_TYPE.Normal);
			int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(spellData);
			character.traitContainer.AddTrait(character, "Spooked", out var trait, null, bypassElementalChance: false, durationBonusPerLevel);
			if (trait is Spooked spooked)
			{
				spooked.AddSourceOfFear(PLAYER_SKILL_TYPE.TERRIFYING_HOWL);
				character.moodComponent.UpdateMoodModificationLog(spooked, spooked.responsibleCharacter);
			}
			character.marker.AddPOIAsInVisionRange(_baseParticleEffect.targetTile.tileObjectComponent.genericTileObject);
			character.combatComponent.Flight(_baseParticleEffect.targetTile.tileObjectComponent.genericTileObject, "Terrifying_Howl");
		}
		else
		{
			character.reactionComponent.ResistRuinarchPower();
		}
		affectedCharacters.Add(character);
	}
}
