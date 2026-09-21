using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class LightningData : SkillData
{
	private Action<ITraitable, int, float> _traitableCallback;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LIGHTNING;

	public override string name => "Lightning";

	public override string description => "This Spell triggers a lightning strike at the target spot. Deals major Electric damage.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 1;

	public LightningData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_traitableCallback = LightningDamage;
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(this);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(this);
		GameObject in_gameObjectID = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Lightning_Strike);
		AkSoundEngine.PostEvent("Play_Lightning", in_gameObjectID);
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		targetTile.PopulateAliveTraitablesOnTile(list);
		List<LocationGridTile> list2 = targetTile.FourNeighbours();
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].PopulateAliveTraitablesOnTile(list);
		}
		TraitManager.Instance.PerformActionOnTraitables(list, damageBaseOnLevel, pierceBasedOnCurrentLevel, _traitableCallback);
		RuinarchListPool<ITraitable>.Release(list);
		targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Lightning Remnant", out var trait);
		if (trait is RemnantTrait remnantTrait)
		{
			remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.LIGHTNING);
		}
		base.ActivateAbility(targetTile);
	}

	private void LightningDamage(ITraitable traitable, int processedDamage, float piercing)
	{
		traitable.AdjustHP(-processedDamage, ELEMENTAL_TYPE.Electric, triggerDeath: true, this, null, showHPBar: true, piercing, isPlayerSource: true);
		if (traitable is Character character)
		{
			character.OnCharacterHitByPlayerSpell(-processedDamage);
			if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
			{
				character.skillCauseOfDeath = PLAYER_SKILL_TYPE.LIGHTNING;
			}
		}
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
