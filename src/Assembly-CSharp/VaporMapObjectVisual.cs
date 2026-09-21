using System.Collections;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class VaporMapObjectVisual : MovingMapObjectVisual
{
	private const float Movement_Speed = 0.3f;

	[SerializeField]
	private ParticleSystem _vaporEffect;

	private string _expiryKey;

	private int _size;

	private Vapor _vapor;

	public bool wasJustPlaced { get; private set; }

	public override void UpdateTileObjectVisual(TileObject obj)
	{
	}

	protected override void Update()
	{
		base.Update();
		if (base.isSpawned)
		{
			if (wasJustPlaced)
			{
				wasJustPlaced = false;
			}
			if (base.gridTileLocation == null)
			{
				Expire();
			}
		}
	}

	public override void Initialize(TileObject obj)
	{
		base.Initialize(obj);
		_vapor = obj as Vapor;
	}

	public override void PlaceObjectAt(LocationGridTile tile)
	{
		base.PlaceObjectAt(tile);
		wasJustPlaced = true;
		if (_vapor.size > 0)
		{
			SetSize(_vapor.size);
		}
		MoveToRandomDirectionInitial(0.3f);
		_expiryKey = SchedulingManager.Instance.AddEntry(_vapor.expiryDate, Expire, this);
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
		_vaporEffect.Clear();
	}

	protected override void OnGamePaused(bool isPaused)
	{
		base.OnGamePaused(isPaused);
		if (isPaused)
		{
			_vaporEffect.Pause();
		}
		else
		{
			_vaporEffect.Play();
		}
	}

	public override void SetWorldPosition(Vector3 worldPosition)
	{
		base.SetWorldPosition(worldPosition);
		if (GameManager.Instance.gameHasStarted)
		{
			MoveToRandomDirectionInitial(0.3f);
		}
	}

	public void SetSize(int size)
	{
		_size = size;
		ChangeScaleBySize();
	}

	private void ChangeScaleBySize()
	{
		base.transform.DOScale(new Vector3(_size, _size, 1f), 1f);
		_vaporEffect.transform.DOScale(new Vector3(_size, _size, _size), 1f);
	}

	public void Expire()
	{
		_vapor.OnExpire();
		_vaporEffect.Stop();
		visionTrigger.SetAllCollidersState(state: false);
		base.isSpawned = false;
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
		}
		_vapor.Expire();
		StartCoroutine(DestroyCoroutine());
	}

	private IEnumerator DestroyCoroutine()
	{
		yield return GameUtilities.waitFor2Seconds;
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	private IEnumerator PlayParticleCoroutineWhenGameIsPaused()
	{
		_vaporEffect.Play();
		yield return GameUtilities.waitForTenthOfSecond;
		_vaporEffect.Pause();
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (base.isSpawned)
		{
			BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is Vapor vapor && vapor != _vapor && !wasJustPlaced)
			{
				CollidedWithVapor(vapor);
			}
		}
	}

	private void CollidedWithVapor(Vapor otherVapor)
	{
		if (!otherVapor.hasExpired && _vapor.size != _vapor.maxSize)
		{
			int stacks = otherVapor.stacks;
			otherVapor.mapVisual.transform.DOKill();
			otherVapor.mapVisual.transform.DOMove(base.transform.position, 4f);
			otherVapor.SetDoExpireEffect(state: false);
			otherVapor.Neutralize();
			_vapor.SetStacks(_vapor.stacks + stacks);
		}
	}
}
