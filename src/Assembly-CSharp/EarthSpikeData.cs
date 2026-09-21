using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class EarthSpikeData : SkillData
{
	private Action<ITraitable, int, float> _traitableCallback;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EARTH_SPIKE;

	public override string name => "Earth Spike";

	public override string description => "This Spell causes a small Earth Spike to erupt from the ground, dealing Earth damage to those within range.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.EARTH_SPIKE);

	public EarthSpikeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_traitableCallback = ApplyEarthDamage;
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(this);
		int num = radius;
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(this);
		GameObject gameObject = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Earth_Spike);
		AkSoundEngine.PostEvent("Play_Earth_Spike", gameObject);
		InnerMapCameraMove.Instance.DoGenericCameraShake();
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		targetTile.PopulateTilesInRadius(list2, num, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * (float)num, gameObject.transform.localScale.y * (float)num, gameObject.transform.localScale.z * (float)num);
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].PopulateAliveTraitablesOnTile(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		TraitManager.Instance.PerformActionOnTraitables(list, damageBaseOnLevel, pierceBasedOnCurrentLevel, _traitableCallback);
		RuinarchListPool<ITraitable>.Release(list);
		targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Danger Remnant", out var trait);
		if (trait is RemnantTrait remnantTrait)
		{
			remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.EARTH_SPIKE);
		}
		base.ActivateAbility(targetTile);
	}

	private void ApplyEarthDamage(ITraitable traitable, int processedDamage, float piercing)
	{
		traitable.AdjustHP(-processedDamage, ELEMENTAL_TYPE.Earth, triggerDeath: true, this, null, showHPBar: true, piercing, isPlayerSource: true);
		if (traitable is Character character)
		{
			character.OnCharacterHitByPlayerSpell(-processedDamage);
			if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
			{
				character.skillCauseOfDeath = PLAYER_SKILL_TYPE.EARTH_SPIKE;
			}
		}
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			return targetTile.structure != null;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
