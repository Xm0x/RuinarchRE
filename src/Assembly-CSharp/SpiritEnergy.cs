using System;
using System.Collections;
using DG.Tweening;
using EZObjectPools;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class SpiritEnergy : PooledObject
{
	private const int ExpiryInHours = 2;

	private string expiryKey;

	private Coroutine positionCoroutine;

	private Vector3 randomPos;

	private Vector3 velocity = Vector3.zero;

	[SerializeField]
	private Collider2D _collider;

	[SerializeField]
	private TrailRenderer _trail;

	private int m_amount;

	public Region location { get; private set; }

	public void Initialize(Region location, int p_amount)
	{
		m_amount = p_amount;
		this.location = location;
		GameDate gameDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
		expiryKey = SchedulingManager.Instance.AddEntry(gameDate, Expire, this);
		randomPos = base.transform.position;
		randomPos.x += UnityEngine.Random.Range(-1.5f, 1.5f);
		randomPos.y += UnityEngine.Random.Range(-1.5f, 1.5f);
		positionCoroutine = StartCoroutine(GoTo(randomPos, 0.5f));
		_collider.enabled = true;
		_trail.enabled = false;
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
		Messenger.Broadcast(PlayerSignals.SPIRIT_ENERGY_EXPIRED, this);
		Destroy();
	}

	private void Destroy()
	{
		if (!string.IsNullOrEmpty(expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	public void OnPointerEnter(BaseEventData data)
	{
		if (positionCoroutine != null)
		{
			StopCoroutine(positionCoroutine);
		}
		_collider.enabled = false;
		_trail.enabled = true;
		Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(PlayerUI.Instance.spiritEnergyLabel.transform.position);
		Vector3 position = base.transform.position;
		position.x += 5f;
		Vector3 vector2 = vector;
		vector2.y -= 5f;
		if (InnerMapCameraMove.Instance.target == base.transform)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(null);
		}
		base.transform.DOPath(new Vector3[3] { vector, position, vector2 }, 0.7f, PathType.CubicBezier).SetEase(Ease.InSine).OnComplete(GainSpiritEnergy);
		Messenger.Broadcast(PlayerSignals.SPIRIT_ENERGY_COLLECTED);
	}

	private void GainSpiritEnergy()
	{
		PlayerManager.Instance.player.currenciesComponent.AdjustSpiritEnergy(m_amount);
		Destroy();
	}

	public override void Reset()
	{
		base.Reset();
		PlayerManager.Instance.RemoveSpiritEnergyFromAvailability(this);
		location = null;
		_trail.Clear();
		_collider.enabled = true;
		positionCoroutine = null;
	}
}
