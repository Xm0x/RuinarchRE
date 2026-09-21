using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class IceBlastData : SkillData
{
	private Action<ITraitable, int, float> _traitableCallback;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ICE_BLAST;

	public override string name => "Ice Blast";

	public override string description => "This Spell causes cold spikes to shatter around a small area, dealing Ice damage to those within range.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.ICE_BLAST);

	public IceBlastData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_traitableCallback = ApplyIceDamage;
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(this);
		int num = radius;
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(this);
		GameObject gameObject = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Ice_Blast);
		AkSoundEngine.PostEvent("Play_Ice_Blast", gameObject);
		gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * (float)num, gameObject.transform.localScale.y * (float)num, gameObject.transform.localScale.z * (float)num);
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		targetTile.PopulateTilesInRadius(list2, num, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
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
			remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.ICE_BLAST);
		}
		base.ActivateAbility(targetTile);
	}

	private void ApplyIceDamage(ITraitable traitable, int processedDamage, float piercing)
	{
		traitable.AdjustHP(-processedDamage, ELEMENTAL_TYPE.Ice, triggerDeath: true, this, null, showHPBar: true, piercing, isPlayerSource: true);
		if (traitable is Character character)
		{
			character.OnCharacterHitByPlayerSpell(-processedDamage);
			if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
			{
				character.skillCauseOfDeath = PLAYER_SKILL_TYPE.ICE_BLAST;
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
