using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public sealed class TornadoMapObjectVisual : MovingMapObjectVisual
{
	[Header("Particles")]
	[SerializeField]
	private ParticleSystem[] particles;

	private float _speed;

	private int _radius;

	private List<IDamageable> _damagablesInTornado;

	private Tornado _tornado;

	private string _expiryKey;

	private uint _sfxID;

	private IDamageable _currentlySuckedInDamageable;

	private TweenCallback _suckedInCallback;

	public override void Initialize(TileObject tileObject)
	{
		base.Initialize(tileObject);
		base.name = tileObject.ToString();
		base.selectable = tileObject;
		_tornado = tileObject as Tornado;
		_radius = _tornado.radius;
		_suckedInCallback = OnDamagableReachedThis;
		if (_damagablesInTornado == null)
		{
			_damagablesInTornado = new List<IDamageable>(50);
		}
		else
		{
			_damagablesInTornado.Clear();
		}
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Tornado_Active", base.gameObject);
	}

	private IEnumerator GamePauseCoroutine()
	{
		yield return null;
		OnGamePaused(GameManager.Instance.isPaused);
	}

	private void PlayTornadoParticle()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Play();
		}
	}

	private void PauseTornadoParticle()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Pause();
		}
	}

	private void StopTornadoParticle()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Stop();
		}
	}

	private void ClearTornadoParticle()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Clear();
		}
	}

	public override void PlaceObjectAt(LocationGridTile tile)
	{
		base.PlaceObjectAt(tile);
		base.isSpawned = true;
		UpdateSpeed();
		MoveToRandomDirectionSpecific(_speed);
		_expiryKey = SchedulingManager.Instance.AddEntry(_tornado.expiryDate, Expire, this);
		Messenger.AddListener(Signals.TICK_ENDED, PerTick);
		Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
	}

	private void MoveToRandomDirectionSpecific(float p_speed, float distance = 50f)
	{
		Vector3 vector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized * distance;
		vector += base.transform.position;
		if (_movement != null)
		{
			_movement.Kill();
		}
		_movement = base.rigidBody.DOMove(vector, p_speed).SetSpeedBased(isSpeedBased: true);
		_movement.Pause();
		UpdateMovementSpeedGivenProgression(GameManager.Instance.currProgressionSpeed);
		StartCoroutine(GamePauseCoroutine());
	}

	private void UpdateSpeed()
	{
		_speed = PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.TORNADO);
	}

	protected override void OnGamePaused(bool isPaused)
	{
		if (base.isSpawned)
		{
			if (isPaused)
			{
				PauseTornadoParticle();
			}
			else
			{
				PlayTornadoParticle();
			}
			UpdateSpeed();
			base.OnGamePaused(isPaused);
		}
	}

	public void Expire()
	{
		StopTornadoParticle();
		SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
		_tornado.Expire();
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	public override void Reset()
	{
		base.Reset();
		base.isSpawned = false;
		_currentlySuckedInDamageable = null;
		_damagablesInTornado.Clear();
		AkSoundEngine.StopPlayingID(_sfxID);
		ClearTornadoParticle();
		Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
		Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
	}

	protected override void Update()
	{
		base.Update();
		if (base.isSpawned && base.gridTileLocation == null)
		{
			Expire();
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable != null)
		{
			AddDamageable(andAddPOIVisionTriggerFromCache.damageable);
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null)
		{
			RemoveDamageable(andAddPOIVisionTriggerFromCache.damageable);
		}
	}

	private void AddDamageable(IDamageable poi)
	{
		if (!(poi is MovingTileObject) && !_damagablesInTornado.Contains(poi))
		{
			_damagablesInTornado.Add(poi);
			OnAddPoiActions(poi);
		}
	}

	private void RemoveDamageable(IDamageable poi)
	{
		_damagablesInTornado.Remove(poi);
		DOTween.Kill(this);
	}

	private void OnAddPoiActions(IDamageable poi)
	{
		if (!(poi is MovingTileObject) && !(poi is Dragon) && (!(poi is TileObject tileObject) || !tileObject.tileObjectType.IsDemonicStructureTileObject()) && !(poi.mapObjectVisual == null) && !(poi.mapObjectVisual.objectSpriteRenderer == null))
		{
			poi.mapObjectVisual.objectSpriteRenderer.transform.DOShakeRotation(20f, new Vector3(0f, 0f, 10f));
		}
	}

	private void PerTick()
	{
		if (!base.isSpawned || !base.gameObject.activeSelf || base.gridTileLocation == null)
		{
			return;
		}
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.TORNADO);
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		base.gridTileLocation.PopulateTilesInRadius(list, _radius, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].tileObjectComponent.genericTileObject.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Wind, triggerDeath: true, this, null, showHPBar: false, pierceBasedOnCurrentLevel, _tornado.isPlayerSource);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		for (int j = 0; j < _damagablesInTornado.Count; j++)
		{
			IDamageable damageable = _damagablesInTornado[j];
			if (damageable.mapObjectVisual != null)
			{
				if ((base.transform.position - damageable.mapObjectVisual.gameObjectVisual.transform.position).magnitude < 3f)
				{
					DealDamage(damageable, damageBaseOnLevel, pierceBasedOnCurrentLevel);
				}
				else if (_currentlySuckedInDamageable == null)
				{
					TrySuckIn(damageable);
				}
				else
				{
					DealDamage(damageable, damageBaseOnLevel, pierceBasedOnCurrentLevel);
				}
			}
		}
	}

	private void DealDamage(IDamageable damageable, int processedDamage, float piercing)
	{
		if (!damageable.CanBeDamaged())
		{
			return;
		}
		if (damageable is Character character)
		{
			damageable.AdjustHP(-processedDamage, ELEMENTAL_TYPE.Wind, triggerDeath: true, _tornado, null, showHPBar: true, piercing, _tornado.isPlayerSource);
			if (_tornado.isPlayerSource)
			{
				character.OnCharacterHitByPlayerSpell(-processedDamage);
			}
			if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
			{
				character.skillCauseOfDeath = PLAYER_SKILL_TYPE.TORNADO;
			}
		}
		else if (!(damageable is TileObject tileObject) || !tileObject.tileObjectType.IsDemonicStructureTileObject())
		{
			int num = Mathf.RoundToInt((float)processedDamage * 0.5f);
			damageable.AdjustHP(-num, ELEMENTAL_TYPE.Wind, triggerDeath: true, _tornado, null, showHPBar: true, piercing, _tornado.isPlayerSource);
		}
	}

	private bool TrySuckIn(IDamageable damageable)
	{
		if (CanBeSuckedIn(damageable) && GameUtilities.RollChance(35))
		{
			_currentlySuckedInDamageable = damageable;
			damageable.mapObjectVisual.objectSpriteRenderer.transform.DOMove(base.transform.position, 10f).SetSpeedBased(isSpeedBased: true).OnComplete(_suckedInCallback);
			if (damageable is IPointOfInterest pointOfInterest)
			{
				pointOfInterest.SetPOIState(POI_STATE.INACTIVE);
			}
			return true;
		}
		return false;
	}

	private void OnDamagableReachedThis()
	{
		if (_currentlySuckedInDamageable != null)
		{
			_currentlySuckedInDamageable.mapObjectVisual?.OnReachTarget();
			_currentlySuckedInDamageable.AdjustHP(-_currentlySuckedInDamageable.maxHP, ELEMENTAL_TYPE.Wind, triggerDeath: true, _tornado, null, showHPBar: true, 0f, _tornado.isPlayerSource);
			_currentlySuckedInDamageable = null;
		}
	}

	private bool CanBeSuckedIn(IDamageable damageable)
	{
		if (damageable is TileObject tileObject && tileObject.tileObjectType.IsDemonicStructureTileObject())
		{
			return false;
		}
		if (damageable is ITraitable traitable && traitable.traitContainer.HasTrait("Immovable"))
		{
			return false;
		}
		if (damageable.CanBeDamaged() && !(damageable is GenericTileObject) && !(damageable is Character))
		{
			return !damageable.mapObjectVisual.IsTweening();
		}
		return false;
	}

	public override void UpdateTileObjectVisual(TileObject obj)
	{
	}

	private void OnTileObjectRemovedFromTile(TileObject tileObject, Character removedBy, LocationGridTile removedFrom)
	{
		RemoveDamageable(tileObject);
	}
}
