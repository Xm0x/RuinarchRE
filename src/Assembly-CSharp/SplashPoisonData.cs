using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class SplashPoisonData : SkillData
{
	private Action<ITraitable> _traitableCallback;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPLASH_POISON;

	public override string name => "Splash Poison";

	public override string description => "This Spell applies Poison on a small area.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_POISON);

	public SplashPoisonData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_traitableCallback = MakeTraitablePoisoned;
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
		GameObject gameObject = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Bomb);
		AkSoundEngine.PostEvent("Play_Splash_Poison", gameObject);
		gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * (float)num, gameObject.transform.localScale.y * (float)num, gameObject.transform.localScale.z * (float)num);
		targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Surprised Remnant", out var trait);
		if (trait is RemnantTrait remnantTrait)
		{
			remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.SPLASH_POISON);
		}
		base.ActivateAbility(targetTile);
	}

	private void MakeTraitablePoisoned(ITraitable traitable)
	{
		bool bypassElementalChance = true;
		if (traitable.traitContainer.HasTrait("Poison Resistant"))
		{
			bypassElementalChance = false;
		}
		traitable.traitContainer.AddTrait(traitable, "Poisoned", null, bypassElementalChance, GetDurationBaseOnTileType(traitable), 0f, ELEMENTAL_TYPE.Poison);
		traitable.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.SetIsPlayerSource(p_state: true);
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

	public int GetDurationBaseOnTileType(ITraitable p_targetTile)
	{
		int result = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_POISON);
		if (p_targetTile is GenericTileObject genericTileObject)
		{
			genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.CLEANSE_TILE);
			if (genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand)
			{
				result = GameManager.Instance.GetTicksBasedOnHour(2);
			}
		}
		return result;
	}
}
