using System;
using System.Collections;
using DG.Tweening;
using EZObjectPools;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class ChaosOrb : PooledObject
{
	private const int ExpiryInHours = 2;

	private string expiryKey;

	private Coroutine positionCoroutine;

	private Vector3 velocity = Vector3.zero;

	public CURRENCY targetCurrency = CURRENCY.Chaotic_Energy;

	[SerializeField]
	private Collider2D _collider;

	[SerializeField]
	private TrailRenderer _trail;

	private ChaosOrbPointer _pointer;

	public Region location { get; private set; }

	public Vector3 randomPos { get; private set; }

	public void Initialize(Region location)
	{
		this.location = location;
		GameDate gameDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
		expiryKey = SchedulingManager.Instance.AddEntry(gameDate, Expire, this);
		randomPos = base.transform.position;
		Vector3 vector = randomPos;
		vector.x += UnityEngine.Random.Range(-1.5f, 1.5f);
		vector.y += UnityEngine.Random.Range(-1.5f, 1.5f);
		randomPos = vector;
		positionCoroutine = StartCoroutine(GoTo(randomPos, 0.5f));
		_collider.enabled = true;
		_trail.enabled = false;
		if (!InnerMapCameraMove.Instance.IsVisible(randomPos))
		{
			_pointer = ChaosOrbParentPointerUI.Instance.ShowChaosOrbPointerOn(randomPos, -1);
		}
	}

	public void Initialize(Vector3 pos, Region location)
	{
		this.location = location;
		GameDate gameDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
		expiryKey = SchedulingManager.Instance.AddEntry(gameDate, Expire, this);
		randomPos = pos;
		base.transform.position = pos;
		_collider.enabled = true;
		_trail.enabled = false;
		if (!InnerMapCameraMove.Instance.IsVisible(randomPos))
		{
			_pointer = ChaosOrbParentPointerUI.Instance.ShowChaosOrbPointerOn(randomPos, -1);
		}
	}

	private IEnumerator GoTo(Vector3 targetPos, float smoothTime, Action onReachAction = null)
	{
		while (!Mathf.Approximately(base.transform.position.x, targetPos.x) && !Mathf.Approximately(base.transform.position.y, targetPos.y))
		{
			base.transform.position = Vector3.SmoothDamp(base.transform.position, targetPos, ref velocity, smoothTime);
			yield return null;
		}
		Vector3 position = base.transform.position;
		base.transform.position = new Vector3(position.x, position.y, -1f);
		onReachAction?.Invoke();
		positionCoroutine = null;
	}

	private void Expire()
	{
		Messenger.Broadcast(PlayerSignals.CHAOS_ORB_EXPIRED, this);
		Destroy();
	}

	private void Destroy()
	{
		if (!string.IsNullOrEmpty(expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
		ObjectPoolManager.Instance.DestroyObject(this);
		DestroyChaosOrbPointer();
	}

	private void DestroyChaosOrbPointer()
	{
		if (_pointer != null && _pointer.gameObject.activeSelf)
		{
			_pointer.HideChaosOrbPointer();
			_pointer = null;
		}
	}

	public void OnPointerEnter(BaseEventData data)
	{
		if (positionCoroutine != null)
		{
			StopCoroutine(positionCoroutine);
		}
		_collider.enabled = false;
		_trail.enabled = true;
		Transform transform = PlayerUI.Instance.plaguePointLbl.transform;
		Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(transform.position);
		Vector3 position = base.transform.position;
		position.x += 5f;
		Vector3 vector2 = vector;
		vector2.y -= 5f;
		if (InnerMapCameraMove.Instance.target == base.transform)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(null);
		}
		DestroyChaosOrbPointer();
		base.transform.DOPath(new Vector3[3] { vector, position, vector2 }, 0.7f, PathType.CubicBezier).SetEase(Ease.InSine).OnComplete(GainPlaguePoints);
		Messenger.Broadcast(PlayerSignals.CHAOS_ORB_COLLECTED);
	}

	private void GainPlaguePoints()
	{
		AudioManager.Instance.PlayParticleMagnet();
		PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(6);
		Destroy();
	}

	public override void Reset()
	{
		base.Reset();
		PlayerManager.Instance.RemoveChaosOrbFromAvailability(this);
		location = null;
		_trail.Clear();
		_collider.enabled = true;
		positionCoroutine = null;
	}
}
