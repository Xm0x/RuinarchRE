using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class FireBallMapObjectVisual : MovingMapObjectVisual
{
	[SerializeField]
	private ParticleSystem _coreEffect;

	[SerializeField]
	private ParticleSystem _flareEffect;

	private string _expiryKey;

	private List<ITraitable> _objsInRange;

	private FireBall owner;

	private uint _sfxID;

	public GameObject circleParticle;

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
		owner = obj as FireBall;
		AkSoundEngine.SetSwitch("Fireball_Switch_Group", "Alive", base.gameObject);
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Fireball", base.gameObject);
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
		owner.expiryDate = GameManager.Instance.Today().AddTicks(PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.FIRE_BALL));
		circleParticle.SetActive(value: true);
		float p_speed = (float)PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.FIRE_BALL) / 100f;
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
		_expiryKey = string.Empty;
		_movement?.Kill();
		_movement = null;
		_objsInRange.Clear();
		_coreEffect.Clear();
		_flareEffect.Clear();
		AkSoundEngine.StopPlayingID(_sfxID);
	}

	protected override void OnGamePaused(bool isPaused)
	{
		base.OnGamePaused(isPaused);
		if (isPaused)
		{
			_coreEffect.Pause();
			_flareEffect.Pause();
		}
		else
		{
			_coreEffect.Play();
			_flareEffect.Play();
		}
	}

	public override void SetWorldPosition(Vector3 worldPosition)
	{
		base.SetWorldPosition(worldPosition);
		if (GameManager.Instance.gameHasStarted)
		{
			float p_speed = (float)PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.FIRE_BALL) / 100f;
			MoveToRandomDirectionInitial(p_speed);
		}
	}

	private void PerTick()
	{
		if (!base.isSpawned)
		{
			return;
		}
		bool flag = false;
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.FIRE_BALL);
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		BurningSource burningSource = null;
		for (int i = 0; i < _objsInRange.Count; i++)
		{
			ITraitable traitable = _objsInRange[i];
			if (owner == traitable)
			{
				continue;
			}
			Character character = traitable as Character;
			if (character != null && character.isDead)
			{
				continue;
			}
			traitable.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Fire, triggerDeath: true, piercingPower: pierceBasedOnCurrentLevel, isPlayerSource: owner.isPlayerSource, source: owner.isPlayerSource ? spellData : null, elementalTraitProcessor: null, showHPBar: true);
			Burning traitOrStatus = traitable.traitContainer.GetTraitOrStatus<Burning>("Burning");
			if (traitOrStatus != null && traitOrStatus.sourceOfBurning == null)
			{
				if (burningSource == null)
				{
					burningSource = new BurningSource();
				}
				traitOrStatus.SetSourceOfBurning(burningSource, traitable);
			}
			if (character != null)
			{
				if (owner.isPlayerSource)
				{
					character.OnCharacterHitByPlayerSpell(-damageBaseOnLevel);
				}
				if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
				{
					character.skillCauseOfDeath = PLAYER_SKILL_TYPE.FIRE_BALL;
				}
			}
			if (!flag && traitable is ThinWall)
			{
				flag = true;
			}
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
		AkSoundEngine.SetSwitch("Fireball_Switch_Group", "Explode", base.gameObject);
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
			traitable2.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Fire, triggerDeath: true, piercingPower: pierceBasedOnCurrentLevel, isPlayerSource: owner.isPlayerSource, source: owner.isPlayerSource ? spellData : null, elementalTraitProcessor: null, showHPBar: true);
			Burning traitOrStatus2 = traitable2.traitContainer.GetTraitOrStatus<Burning>("Burning");
			if (traitOrStatus2 != null && traitOrStatus2.sourceOfBurning == null)
			{
				if (burningSource == null)
				{
					burningSource = new BurningSource();
				}
				traitOrStatus2.SetSourceOfBurning(burningSource, traitable2);
			}
			if (traitable2 is Character character2)
			{
				if (owner.isPlayerSource)
				{
					character2.OnCharacterHitByPlayerSpell(-damageBaseOnLevel);
				}
				if (character2.isDead && character2.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
				{
					character2.skillCauseOfDeath = PLAYER_SKILL_TYPE.FIRE_BALL;
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		RuinarchListPool<ITraitable>.Release(list);
		GameManager.Instance.CreateParticleEffectAt(locationGridTile, PARTICLE_EFFECT.Fire_Explosion);
		Expire();
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (base.isSpawned)
		{
			BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable && !(traitable is FireBall))
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
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable && !(traitable is FireBall))
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
		_coreEffect.Stop();
		_flareEffect.Stop();
		base.isSpawned = false;
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
		}
		circleParticle.SetActive(value: false);
		Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
		owner.Expire();
		StartCoroutine(DestroyCoroutine());
	}

	private IEnumerator DestroyCoroutine()
	{
		yield return GameUtilities.waitFor1Second;
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	private IEnumerator PlayParticleCoroutineWhenGameIsPaused()
	{
		_coreEffect.Play();
		_flareEffect.Play();
		yield return GameUtilities.waitForTenthOfSecond;
		_coreEffect.Pause();
		_flareEffect.Pause();
	}
}
