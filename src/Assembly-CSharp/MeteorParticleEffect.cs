using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;

public class MeteorParticleEffect : BaseParticleEffect
{
	public ParticleSystem meteorParticle;

	public bool isPlayerSource;

	private bool hasMeteorFell;

	protected override IEnumerator PlayParticleCoroutine()
	{
		PlayParticle();
		yield return GameUtilities.waitForTenthOfSecond;
		if ((pauseOnGamePaused && GameManager.Instance.isPaused) || !GameManager.Instance.gameHasStarted)
		{
			PauseParticle();
		}
	}

	protected override void ResetParticle()
	{
		base.ResetParticle();
		hasMeteorFell = false;
	}

	private void OnMeteorFell()
	{
		hasMeteorFell = true;
		AkSoundEngine.PostEvent("Play_Meteor", base.gameObject);
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.METEOR);
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		BurningSource bs = null;
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		base.targetTile.PopulateAliveTraitablesOnTile(list);
		for (int i = 0; i < base.targetTile.neighbourList.Count; i++)
		{
			base.targetTile.neighbourList[i].PopulateAliveTraitablesOnTile(list);
		}
		if (!isPlayerSource)
		{
			List<LocationStructure> list2 = RuinarchListPool<LocationStructure>.Claim();
			for (int j = 0; j < list.Count; j++)
			{
				if (!(list[j] is TileObject tileObject) || (!tileObject.tileObjectType.IsDemonicStructureTileObject() && tileObject.tileObjectType != TILE_OBJECT_TYPE.STRUCTURE_BLOCKER_TILE_OBJECT))
				{
					continue;
				}
				LocationStructure currentStructure = tileObject.currentStructure;
				if (currentStructure != null)
				{
					if (list2.Contains(currentStructure))
					{
						list.RemoveAt(j);
						j--;
					}
					else
					{
						list2.Add(currentStructure);
					}
				}
			}
			RuinarchListPool<LocationStructure>.Release(list2);
		}
		for (int k = 0; k < list.Count; k++)
		{
			ITraitable traitable = list[k];
			MeteorEffect(traitable, damageBaseOnLevel, pierceBasedOnCurrentLevel, spellData, ref bs);
		}
		RuinarchListPool<ITraitable>.Release(list);
		base.targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(base.targetTile.tileObjectComponent.genericTileObject, "Danger Remnant", out var trait);
		if (trait is RemnantTrait remnantTrait)
		{
			remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.METEOR);
		}
		AkSoundEngine.PostEvent("Play_Meteor", base.targetTile.tileObjectComponent.genericTileObject.mapObjectVisual.gameObject);
		Messenger.Broadcast(PlayerSkillSignals.METEOR_FELL);
		InnerMapCameraMove.Instance.MeteorShake();
		if (isPlayerSource)
		{
			base.targetTile.RemoveMeteor();
		}
		else
		{
			base.targetTile.RemoveNonPlayerMeteor();
		}
	}

	private void MeteorEffect(ITraitable traitable, int processedDamage, float piercing, SkillData meteorData, ref BurningSource bs)
	{
		if (traitable.gridTileLocation == null)
		{
			return;
		}
		BurningSource burningSource = bs;
		traitable.AdjustHP(-processedDamage, ELEMENTAL_TYPE.Fire, triggerDeath: true, piercingPower: piercing, isPlayerSource: isPlayerSource, source: isPlayerSource ? meteorData : null, elementalTraitProcessor: delegate(ITraitable target, Trait trait)
		{
			TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource);
		}, showHPBar: true);
		if (isPlayerSource && traitable is Character character)
		{
			character.OnCharacterHitByPlayerSpell(-processedDamage);
			if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
			{
				character.skillCauseOfDeath = PLAYER_SKILL_TYPE.METEOR;
			}
		}
		bs = burningSource;
	}

	private void OnTweenComplete()
	{
		InnerMapCameraMove.Instance.camera.transform.DORotate(new Vector3(0f, 0f, 0f), 0.2f);
	}

	private IEnumerator ExpireCoroutine(GameObject go)
	{
		yield return GameUtilities.waitFor2Seconds;
		ObjectPoolManager.Instance.DestroyObject(go);
	}

	private void Update()
	{
		if (!hasMeteorFell)
		{
			if (meteorParticle.isStopped)
			{
				OnMeteorFell();
			}
			return;
		}
		bool flag = true;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			if (particleSystems[i].IsAlive())
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			ObjectPoolManager.Instance.DestroyObject(base.gameObject);
		}
	}
}
