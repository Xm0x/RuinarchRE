using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class SplashWaterData : SkillData
{
	private Action<ITraitable> _traitableCallback;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPLASH_WATER;

	public override string name => "Splash Water";

	public override string description => "This Spell applies Wet to a small area.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_WATER);

	public SplashWaterData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_traitableCallback = MakeTraitableWet;
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		int num = radius;
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		targetTile.PopulateTilesInRadius(list2, num, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].PopulateTraitablesOnTile(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		TraitManager.Instance.PerformActionOnTraitables(list, _traitableCallback);
		RuinarchListPool<ITraitable>.Release(list);
		GameObject gameObject = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Water_Bomb);
		AkSoundEngine.PostEvent("Play_Splash_Water", gameObject);
		gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * (float)num, gameObject.transform.localScale.y * (float)num, gameObject.transform.localScale.z * (float)num);
		targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Surprised Remnant", out var trait);
		if (trait is RemnantTrait remnantTrait)
		{
			remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.SPLASH_WATER);
		}
		base.ActivateAbility(targetTile);
	}

	private void MakeTraitableWet(ITraitable traitable)
	{
		int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_WATER);
		traitable.traitContainer.AddTrait(traitable, "Wet", null, bypassElementalChance: true, durationBonusPerLevel, 0f, ELEMENTAL_TYPE.Water);
		if (traitable == null || traitable.gridTileLocation == null)
		{
			return;
		}
		Wet traitOrStatus = traitable.traitContainer.GetTraitOrStatus<Wet>("Wet");
		if (traitOrStatus != null)
		{
			traitOrStatus.SetIsPlayerSource(p_state: true);
			if (traitable is Bed bed)
			{
				bed.WakeUpUsersBecauseOfWet();
			}
			else if (traitable is Character character && character.traitContainer.HasTrait("Resting"))
			{
				character.currentJob?.CancelJob();
				character.traitContainer.AddTrait(character, "Irate");
			}
			traitOrStatus.PerformTreeRoll();
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
