using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class BallLightningMapObjectVisual : MovingMapObjectVisual
{
	[SerializeField]
	private ParticleSystem _ballLightningEffect;

	public GameObject circleParticle;

	private string _expiryKey;

	private List<ITraitable> _objsInRange;

	private BallLightning owner;

	private uint _sfxID;

	public override void UpdateTileObjectVisual(TileObject obj)
	{
	}

	protected override void Update()
	{
		base.Update();
		if (base.isSpawned && base.gridTileLocation == null)
		{
			Expire();
		}
	}

	private void OnDisable()
	{
		AkSoundEngine.StopAll(base.gameObject);
	}

	public override void Initialize(TileObject obj)
	{
		base.Initialize(obj);
		if (_objsInRange == null)
		{
			_objsInRange = new List<ITraitable>(50);
		}
		else
		{
			_objsInRange.Clear();
		}
		owner = obj as BallLightning;
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Ball_Lightning", base.gameObject);
	}

	public override void PlaceObjectAt(LocationGridTile tile)
	{
		base.PlaceObjectAt(tile);
		if (owner.targetDirection == Vector3.zero)
		{
			StartMoving(owner.targetDirection, p_moveToRandom: true);
		}
		else
		{
			StartMoving(owner.targetDirection, p_moveToRandom: false);
		}
	}

	private void StartMoving(Vector3 p_targetDirection, bool p_moveToRandom)
	{
		owner.expiryDate = GameManager.Instance.Today().AddTicks(PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BALL_LIGHTNING));
		circleParticle.SetActive(value: true);
		float p_speed = (float)PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.BALL_LIGHTNING) / 100f;
		if (!p_moveToRandom)
		{
			MoveToDirection(p_targetDirection, p_speed, 200f);
		}
		else
		{
			MoveToRandomDirectionInitial(p_speed);
		}
		_expiryKey = SchedulingManager.Instance.AddEntry(owner.expiryDate, Expire, this);
		Messenger.AddListener(Signals.TICK_ENDED, PerTick);
		base.isSpawned = true;
		if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted)
		{
			StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
		}
	}

	public override void Reset()
	{
		base.Reset();
		base.isSpawned = false;
		_expiryKey = string.Empty;
		_movement?.Kill();
		_movement = null;
		_objsInRange.Clear();
		_ballLightningEffect.Clear();
		AkSoundEngine.StopPlayingID(_sfxID);
	}

	protected override void OnGamePaused(bool isPaused)
	{
		base.OnGamePaused(isPaused);
		if (isPaused)
		{
			_ballLightningEffect.Pause();
		}
		else
		{
			_ballLightningEffect.Play();
		}
	}

	private void PerTick()
	{
		if (!base.isSpawned)
		{
			return;
		}
		bool flag = false;
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.BALL_LIGHTNING);
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		for (int i = 0; i < _objsInRange.Count; i++)
		{
			ITraitable traitable = _objsInRange[i];
			if (owner == traitable)
			{
				continue;
			}
			if (!(traitable is Character character))
			{
				traitable.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Electric, triggerDeath: true, piercingPower: pierceBasedOnCurrentLevel, isPlayerSource: owner.isPlayerSource, source: owner.isPlayerSource ? spellData : null, elementalTraitProcessor: null, showHPBar: true);
				continue;
			}
			if (character.isDead)
			{
				flag = true;
				continue;
			}
			traitable.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Electric, triggerDeath: true, piercingPower: pierceBasedOnCurrentLevel, isPlayerSource: owner.isPlayerSource, source: owner.isPlayerSource ? spellData : null, elementalTraitProcessor: null, showHPBar: true);
			if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Electric_Balls_Chance)) && spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.May_Paralyze))
			{
				PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PARALYSIS).ActivateAbility(traitable as IPointOfInterest);
			}
			if (owner.isPlayerSource)
			{
				character.OnCharacterHitByPlayerSpell(-damageBaseOnLevel);
			}
			if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
			{
				character.skillCauseOfDeath = PLAYER_SKILL_TYPE.BALL_LIGHTNING;
			}
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		LocationGridTile locationGridTile = owner.gridTileLocation;
		if (locationGridTile == null)
		{
			return;
		}
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		locationGridTile.PopulateNeighbours(list2);
		for (int j = 0; j < list2.Count; j++)
		{
			list2[j].PopulateAliveTraitablesOnTile(list);
		}
		for (int k = 0; k < list.Count; k++)
		{
			ITraitable traitable2 = list[k];
			traitable2.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Electric, triggerDeath: true, piercingPower: pierceBasedOnCurrentLevel, isPlayerSource: owner.isPlayerSource, source: owner.isPlayerSource ? spellData : null, elementalTraitProcessor: null, showHPBar: true);
			if (traitable2 is Character character2)
			{
				if (owner.isPlayerSource)
				{
					character2.OnCharacterHitByPlayerSpell(-damageBaseOnLevel);
				}
				if (character2.isDead && character2.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
				{
					character2.skillCauseOfDeath = PLAYER_SKILL_TYPE.BALL_LIGHTNING;
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		RuinarchListPool<ITraitable>.Release(list);
		GameManager.Instance.CreateParticleEffectAt(locationGridTile, PARTICLE_EFFECT.Lightning_Explosion);
		Expire();
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (base.isSpawned)
		{
			BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable)
			{
				AddObject(traitable);
			}
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		if (base.isSpawned)
		{
			BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable)
			{
				RemoveObject(traitable);
			}
		}
	}

	private void AddObject(ITraitable obj)
	{
		if (!_objsInRange.Contains(obj))
		{
			_objsInRange.Add(obj);
		}
	}

	private void RemoveObject(ITraitable obj)
	{
		_objsInRange.Remove(obj);
	}

	public void Expire()
	{
		_ballLightningEffect.Stop();
		base.isSpawned = false;
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
		}
		Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
		owner.Expire();
		StartCoroutine(DestroyCoroutine());
		circleParticle.gameObject.SetActive(value: false);
	}

	private IEnumerator DestroyCoroutine()
	{
		yield return GameUtilities.waitFor1Second;
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	private IEnumerator PlayParticleCoroutineWhenGameIsPaused()
	{
		_ballLightningEffect.Play();
		yield return GameUtilities.waitForTenthOfSecond;
		_ballLightningEffect.Pause();
	}
}
