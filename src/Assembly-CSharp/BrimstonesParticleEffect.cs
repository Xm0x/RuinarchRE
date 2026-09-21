using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class BrimstonesParticleEffect : BaseParticleEffect
{
	public bool IsCastedByPlayer { get; set; }

	protected override void PlayParticle()
	{
		base.PlayParticle();
		AkSoundEngine.PostEvent("Play_Brimstone_Woosh", base.gameObject);
		StartCoroutine(BrimstoneEffect());
	}

	private IEnumerator BrimstoneEffect()
	{
		yield return GameUtilities.waitForHalfSecond;
		AkSoundEngine.PostEvent("Play_Brimstone_Impact", base.gameObject);
		OnBrimstoneFell();
	}

	protected virtual void ParticleAfterEffect(ParticleSystem particleSystem)
	{
		ObjectPoolManager.Instance.DestroyObject(base.gameObject);
	}

	public void OnBrimstoneFell()
	{
		base.targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(base.targetTile.tileObjectComponent.genericTileObject, "Danger Remnant", out var trait);
		if (trait is RemnantTrait remnantTrait)
		{
			remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.BRIMSTONES);
		}
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.BRIMSTONES);
		float piercing = 0f;
		int damageBaseOnLevel;
		if (!IsCastedByPlayer)
		{
			damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData, 0);
		}
		else
		{
			damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
			piercing = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		}
		BurningSource bs = null;
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		base.targetTile.PopulateAliveTraitablesOnTile(list);
		for (int i = 0; i < list.Count; i++)
		{
			ITraitable traitable = list[i];
			BrimstoneEffect(traitable, damageBaseOnLevel, piercing, spellData, ref bs);
		}
		RuinarchListPool<ITraitable>.Release(list);
		if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Creates_Cinders))
		{
			SpawnCinder(base.targetTile, spellData.currentLevel);
		}
	}

	private void BrimstoneEffect(ITraitable traitable, int additionalDamage, float piercing, SkillData brimstonesData, ref BurningSource bs)
	{
		if (traitable is TileObject tileObject)
		{
			if (tileObject.tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT)
			{
				BurningSource burningSource = bs;
				CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Fire, tileObject, null, delegate(ITraitable target, Trait trait)
				{
					TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource);
				}, createHitEffect: true, setAsPlayerSource: false, piercing);
				bs = burningSource;
			}
			else
			{
				BurningSource burningSource2 = bs;
				tileObject.AdjustHP(-additionalDamage, ELEMENTAL_TYPE.Fire, triggerDeath: true, piercingPower: piercing, isPlayerSource: IsCastedByPlayer, source: IsCastedByPlayer ? brimstonesData : null, elementalTraitProcessor: delegate(ITraitable target, Trait trait)
				{
					TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource2);
				}, showHPBar: true);
				bs = burningSource2;
			}
		}
		else if (traitable is Character character)
		{
			BurningSource burningSource3 = bs;
			character.AdjustHP(-additionalDamage, ELEMENTAL_TYPE.Fire, triggerDeath: true, piercingPower: piercing, isPlayerSource: IsCastedByPlayer, source: IsCastedByPlayer ? brimstonesData : null, elementalTraitProcessor: delegate(ITraitable target, Trait trait)
			{
				TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource3);
			}, showHPBar: true);
			bs = burningSource3;
			if (IsCastedByPlayer)
			{
				character.OnCharacterHitByPlayerSpell(-additionalDamage);
			}
			if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
			{
				character.skillCauseOfDeath = PLAYER_SKILL_TYPE.BRIMSTONES;
			}
			if (GameUtilities.RollChance(25))
			{
				character.traitContainer.AddTrait(character, "Injured");
			}
		}
		else
		{
			BurningSource burningSource4 = bs;
			traitable.AdjustHP(-additionalDamage, ELEMENTAL_TYPE.Fire, triggerDeath: true, piercingPower: piercing, isPlayerSource: IsCastedByPlayer, source: IsCastedByPlayer ? brimstonesData : null, elementalTraitProcessor: delegate(ITraitable target, Trait trait)
			{
				TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource4);
			}, showHPBar: true);
			bs = burningSource4;
		}
	}

	private void SpawnCinder(LocationGridTile p_targetTile, int p_level)
	{
		if (p_targetTile.specificBiomeTileType == Biome_Tile_Type.Snow || p_targetTile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Wet") || !GameUtilities.RollChance(50))
		{
			return;
		}
		bool flag = false;
		TileObject objHere = p_targetTile.tileObjectComponent.objHere;
		if (objHere == null)
		{
			flag = true;
		}
		else if (!objHere.tileObjectType.IsTileObjectImportant() && objHere.CanBeDamaged())
		{
			p_targetTile.structure.RemovePOI(objHere);
			flag = true;
		}
		if (flag)
		{
			Cinder cinder = InnerMapManager.Instance.CreateNewTileObject<Cinder>(TILE_OBJECT_TYPE.CINDER);
			cinder.SetLevel(p_level);
			GameDate expiry = GameManager.Instance.Today();
			int hours = 2;
			switch (p_level)
			{
			case 1:
				hours = 3;
				break;
			case 2:
				hours = 4;
				break;
			case 3:
				hours = 5;
				break;
			}
			p_targetTile.structure.AddPOI(cinder, p_targetTile);
			expiry.AddTicks(GameManager.Instance.GetTicksBasedOnHour(hours));
			cinder.SetExpiry(expiry);
		}
	}

	public override void Reset()
	{
		base.Reset();
		IsCastedByPlayer = false;
	}
}
