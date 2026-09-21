using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class IceteroidParticleEffect : BaseParticleEffect
{
	protected override void PlayParticle()
	{
		base.PlayParticle();
		StartCoroutine(IceteroidEffect());
	}

	private IEnumerator IceteroidEffect()
	{
		yield return GameUtilities.waitFor1AndHalfSecond;
		OnIceteroidFell();
	}

	protected virtual void ParticleAfterEffect(ParticleSystem particleSystem)
	{
		ObjectPoolManager.Instance.DestroyObject(base.gameObject);
	}

	public void OnIceteroidFell()
	{
		base.targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(base.targetTile.tileObjectComponent.genericTileObject, "Danger Remnant", out var trait);
		if (trait is RemnantTrait remnantTrait)
		{
			remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.ICETEROIDS);
		}
		AkSoundEngine.PostEvent("Play_Iceteroids", base.targetTile.tileObjectComponent.genericTileObject.mapObjectVisual.gameObject);
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.ICETEROIDS);
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		base.targetTile.PopulateAliveTraitablesOnTile(list);
		for (int i = 0; i < list.Count; i++)
		{
			ITraitable traitable = list[i];
			DealDamage(traitable, damageBaseOnLevel, pierceBasedOnCurrentLevel, spellData);
		}
		RuinarchListPool<ITraitable>.Release(list);
		if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Creates_Ice_Blocks))
		{
			SpawnIceBlock(base.targetTile, spellData.currentLevel);
		}
	}

	private void SpawnIceBlock(LocationGridTile p_targetTile, int p_level)
	{
		if (p_targetTile.specificBiomeTileType != Biome_Tile_Type.Desert && GameUtilities.RollChance(50))
		{
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
				p_targetTile.tileObjectComponent.genericTileObject.traitContainer.RemoveTrait(p_targetTile.tileObjectComponent.genericTileObject, "Burning");
				IceBlockWall iceBlockWall = InnerMapManager.Instance.CreateNewTileObject<IceBlockWall>(TILE_OBJECT_TYPE.ICE_BLOCK_WALL);
				GameDate expiry = GameManager.Instance.Today();
				int hours = 4;
				expiry.AddTicks(GameManager.Instance.GetTicksBasedOnHour(hours));
				iceBlockWall.SetExpiry(expiry);
				p_targetTile.structure.AddPOI(iceBlockWall, p_targetTile);
			}
		}
	}

	private void DealDamage(ITraitable traitable, int additionalDamage, float piercing, SkillData iceteroidsData)
	{
		traitable.AdjustHP(-additionalDamage, ELEMENTAL_TYPE.Ice, triggerDeath: true, iceteroidsData, null, showHPBar: true, piercing, isPlayerSource: true);
		if (traitable is Character { isDead: false } character)
		{
			character.traitContainer.RemoveStatusAndStacks(character, "Freezing");
			character.traitContainer.AddTrait(character, "Frozen", null, bypassElementalChance: true);
			traitable.traitContainer.GetTraitOrStatus<Frozen>("Frozen")?.SetIsPlayerSource(p_state: true);
			if (Random.Range(0, 100) < 25)
			{
				character.traitContainer.AddTrait(character, "Injured");
			}
			character.OnCharacterHitByPlayerSpell(-additionalDamage);
			if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
			{
				character.skillCauseOfDeath = PLAYER_SKILL_TYPE.ICETEROIDS;
			}
		}
	}
}
