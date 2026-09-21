using System;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField]
	private Collider2D _collider;

	[SerializeField]
	private Rigidbody2D _rigidbody;

	[SerializeField]
	private ParticleSystem projectileParticles;

	[SerializeField]
	private ParticleSystem collisionParticles;

	[SerializeField]
	private ParticleCallback collisionParticleCallback;

	[SerializeField]
	private TrailRenderer _lineRenderer;

	public ELEMENTAL_TYPE projectileElement;

	public ELEMENTAL_TYPE poolElement;

	public bool isDragonProjectile;

	public bool isAOE;

	public Action<Character, IDamageable, CombatState, Projectile> onHitAction;

	private Vector3 _pausedVelocity;

	private float _pausedAngularVelocity;

	private CombatState createdBy;

	private Character actor;

	private Tweener tween;

	private bool _hasHit;

	private float _timeAlive;

	private TweenCallback _projectileHitCallback;

	public IDamageable targetObject { get; private set; }

	public Character source => actor;

	private void OnDestroy()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
	}

	private void Update()
	{
		_timeAlive += Time.deltaTime;
		if (_timeAlive > 1f)
		{
			DestroyProjectile();
		}
	}

	private void Awake()
	{
		_projectileHitCallback = OnProjectileHit;
	}

	public void SetTarget(Transform target, IDamageable targetObject, CombatState createdBy, Character shooter)
	{
		SetTarget(target.position, targetObject, createdBy, shooter);
	}

	public void SetTarget(Vector3 targetPosition, IDamageable targetObject, CombatState createdBy, Character shooter)
	{
		_hasHit = false;
		this.targetObject = targetObject;
		this.createdBy = createdBy;
		actor = shooter;
		_timeAlive = 0f;
		if (projectileParticles != null)
		{
			projectileParticles.Play();
		}
		if (targetObject is Character)
		{
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		}
		else if (targetObject is TileObject)
		{
			Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		}
		collisionParticleCallback.SetAction(DestroyProjectile);
		tween = _rigidbody.DOMove(targetPosition, 25f).SetSpeedBased(isSpeedBased: true).SetEase(Ease.Linear)
			.SetAutoKill(autoKillOnCompletion: true);
		if (targetObject is GenericTileObject || targetObject.projectileReceiver == null)
		{
			tween.OnComplete(_projectileHitCallback);
		}
		_lineRenderer.enabled = true;
	}

	public void OnProjectileHit()
	{
		OnProjectileHit(targetObject);
	}

	public void OnProjectileHit(IDamageable poi)
	{
		_hasHit = true;
		tween?.Kill();
		if (projectileParticles != null)
		{
			projectileParticles.Stop();
		}
		onHitAction?.Invoke(actor, poi, createdBy, this);
		_collider.enabled = false;
		collisionParticles.Play(withChildren: true);
	}

	private void DestroyProjectile()
	{
		ObjectPoolManager.Instance.ReturnProjectileToPool(this);
	}

	public void Reset()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		_collider.enabled = true;
		tween?.Kill();
		tween = null;
		if (projectileParticles != null)
		{
			projectileParticles.Stop();
			projectileParticles.Clear();
		}
		collisionParticles.Clear();
		onHitAction = null;
		_timeAlive = 0f;
		_lineRenderer.Clear();
		_lineRenderer.enabled = false;
		actor = null;
		createdBy = null;
	}

	private void OnCharacterAreaTravelling(Character travellingCharacter)
	{
		if (targetObject is Character && (travellingCharacter == targetObject || travellingCharacter.carryComponent.carriedPOI == targetObject))
		{
			DestroyProjectile();
		}
	}

	private void OnCharacterDied(Character character)
	{
		if (character == targetObject)
		{
			DestroyProjectile();
		}
	}

	private void OnTileObjectRemoved(TileObject obj, Character removedBy, LocationGridTile removedFrom)
	{
		if (obj == targetObject)
		{
			DestroyProjectile();
		}
	}
}
