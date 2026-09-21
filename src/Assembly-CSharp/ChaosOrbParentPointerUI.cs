using System.Collections.Generic;
using UnityEngine;

public class ChaosOrbParentPointerUI : MonoBehaviour
{
	public static ChaosOrbParentPointerUI Instance;

	[SerializeField]
	private RectTransform _parentRectTransform;

	[SerializeField]
	private GameObject _chaosOrbPointerPrefab;

	[SerializeField]
	private float _yOffsetArrowPosition;

	[SerializeField]
	private int _initialPointerPoolPrebakedValue;

	private Queue<ChaosOrbPointer> _pointerPool = new Queue<ChaosOrbPointer>(50);

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		InitializePool();
	}

	private void InitializePool()
	{
		for (int i = 0; i < _initialPointerPoolPrebakedValue; i++)
		{
			ChaosOrbPointer item = CreateChaosOrbPointer();
			_pointerPool.Enqueue(item);
		}
	}

	private ChaosOrbPointer GetChaosOrbPointerFromPool()
	{
		if (_pointerPool.Count > 0)
		{
			return _pointerPool.Dequeue();
		}
		return CreateChaosOrbPointer();
	}

	private ChaosOrbPointer CreateChaosOrbPointer()
	{
		GameObject obj = Object.Instantiate(_chaosOrbPointerPrefab, base.transform);
		obj.transform.position = Vector3.zero;
		obj.SetActive(value: false);
		ChaosOrbPointer component = obj.GetComponent<ChaosOrbPointer>();
		component.Initialize(_parentRectTransform, _yOffsetArrowPosition);
		return component;
	}

	public void ReturnChaosOrbPointerToPool(ChaosOrbPointer p_pointer)
	{
		p_pointer.ReturnGameObjectToPool();
		p_pointer.gameObject.SetActive(value: false);
		_pointerPool.Enqueue(p_pointer);
	}

	public ChaosOrbPointer ShowChaosOrbPointerOn(Vector3 p_targetPosition, int p_durationInSeconds)
	{
		ChaosOrbPointer chaosOrbPointerFromPool = GetChaosOrbPointerFromPool();
		chaosOrbPointerFromPool.ShowChaosOrbPointerOn(p_targetPosition, p_durationInSeconds);
		return chaosOrbPointerFromPool;
	}
}
