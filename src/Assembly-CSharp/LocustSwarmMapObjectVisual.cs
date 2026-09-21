using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Ruinarch;
using Traits;
using UnityEngine;
using UtilityScripts;

public class LocustSwarmMapObjectVisual : MovingMapObjectVisual
{
	private string _expiryKey;

	private string _movementKey;

	private List<ITraitable> _objsInRange;

	private LocustSwarm _locustSwarm;

	private uint _sfxID;

	public ParticleSystem locustSwarmParticle;

	private Vector3 previousMousePos;

	public override void UpdateTileObjectVisual(TileObject obj)
	{
	}

	protected override void Update()
	{
		base.Update();
		if (!base.isSpawned)
		{
			return;
		}
		if (base.gridTileLocation == null)
		{
			Expire();
		}
		else
		{
			if (_locustSwarm != PlayerManager.Instance.player.playerSkillComponent.latestCastMovingTileObject || GameManager.Instance.isPaused)
			{
				return;
			}
			Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
			float num = Vector2.Distance(vector, base.worldPos);
			bool flag = _locustSwarm.SetMovementType((num <= 10f) ? Moving_Object_Movement_Type.Follow_Cursor : Moving_Object_Movement_Type.Default);
			switch (_locustSwarm.currentMovementType)
			{
			case Moving_Object_Movement_Type.Default:
				if (flag)
				{
					MoveToRandomDirection();
				}
				break;
			case Moving_Object_Movement_Type.Follow_Cursor:
				if (!string.IsNullOrEmpty(_movementKey))
				{
					SchedulingManager.Instance.RemoveSpecificEntry(_movementKey);
					_movementKey = string.Empty;
				}
				if (vector != previousMousePos)
				{
					MoveToCursor(vector);
					previousMousePos = vector;
				}
				break;
			}
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
		_locustSwarm = obj as LocustSwarm;
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Locust_Swarm", base.gameObject);
	}

	public override void PlaceObjectAt(LocationGridTile tile)
	{
		base.PlaceObjectAt(tile);
		MoveToRandomDirection();
		_expiryKey = SchedulingManager.Instance.AddEntry(_locustSwarm.expiryDate, Expire, this);
		Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted)
		{
			StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
		}
		base.isSpawned = true;
	}

	public override void Reset()
	{
		base.Reset();
		previousMousePos = Vector3.zero;
		base.isSpawned = false;
		_expiryKey = string.Empty;
		if (!string.IsNullOrEmpty(_movementKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_movementKey);
		}
		_movementKey = string.Empty;
		if (_movement != null)
		{
			DOTween.Kill(_movement);
			_movement = null;
		}
		AkSoundEngine.StopPlayingID(_sfxID);
		DOTween.Kill(this);
		DOTween.Kill(base.transform);
		_objsInRange.Clear();
		Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
	}

	protected override void OnGamePaused(bool isPaused)
	{
		base.OnGamePaused(isPaused);
		if (isPaused)
		{
			locustSwarmParticle.Pause();
		}
		else
		{
			locustSwarmParticle.Play();
		}
	}

	protected override void MoveToRandomDirectionBase(float p_speed, float distance = 50f)
	{
		base.MoveToRandomDirectionBase(p_speed, distance);
		_movementKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(20), MoveToRandomDirection, this);
	}

	private void MoveToRandomDirection()
	{
		float p_speed = (float)PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.LOCUST_SWARM) / 100f;
		MoveToRandomDirection(p_speed);
	}

	private void MoveToCursor(Vector3 p_mousePosition)
	{
		float p_speed = (float)PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.LOCUST_SWARM) / 100f;
		MoveToCursor(p_mousePosition, p_speed);
		if (!string.IsNullOrEmpty(_movementKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_movementKey);
			_movementKey = string.Empty;
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable && CanBeAffectedByLocustSwarm(traitable))
		{
			AddObject(traitable);
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable && CanBeAffectedByLocustSwarm(traitable))
		{
			RemoveObject(traitable);
		}
	}

	private void AddObject(ITraitable obj)
	{
		if (!_objsInRange.Contains(obj))
		{
			_objsInRange.Add(obj);
			CheckObjectForEffects(obj);
		}
	}

	private void RemoveObject(ITraitable obj)
	{
		_objsInRange.Remove(obj);
	}

	private void OnTraitableGainedTrait(ITraitable traitable, Trait trait)
	{
		if (CanBeAffectedByLocustSwarm(traitable) && _objsInRange.Contains(traitable) && (trait is Burning || trait.name == "Edible"))
		{
			CheckObjectForEffects(traitable);
		}
	}

	private void CheckObjectForEffects(ITraitable obj)
	{
		if (obj.traitContainer.GetTraitOrStatus<Trait>("Edible") != null || obj is Crops)
		{
			obj.AdjustHP(-obj.currentHP, ELEMENTAL_TYPE.Normal, triggerDeath: true, _locustSwarm, null, showHPBar: true, 0f, isPlayerSource: true);
		}
		if (obj.traitContainer.GetTraitOrStatus<Trait>("Burning") != null)
		{
			_locustSwarm.AdjustHP(-Mathf.FloorToInt((float)_locustSwarm.maxHP * 0.2f), ELEMENTAL_TYPE.Fire, triggerDeath: true, obj);
			obj.traitContainer.RemoveTrait(obj, "Burning");
		}
	}

	private bool CanBeAffectedByLocustSwarm(ITraitable traitable)
	{
		if (traitable is Character)
		{
			return false;
		}
		return true;
	}

	public void Expire()
	{
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
		}
		if (!string.IsNullOrEmpty(_movementKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_movementKey);
		}
		base.isSpawned = false;
		_locustSwarm.Expire();
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	private IEnumerator PlayParticleCoroutineWhenGameIsPaused()
	{
		locustSwarmParticle.Play();
		yield return GameUtilities.waitForTenthOfSecond;
		locustSwarmParticle.Pause();
	}

	public void OnNoLongerLatestCastMovingObject()
	{
		MoveToRandomDirection();
	}
}
