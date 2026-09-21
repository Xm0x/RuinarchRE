using System.Collections;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public abstract class MovingMapObjectVisual : MapObjectVisual<TileObject>
{
	[SerializeField]
	private Rigidbody2D _rigidbody;

	protected const float Follow_Cursor_Distance = 10f;

	protected Tweener _movement;

	protected Region _mapLocation;

	public bool isSpawned { get; protected set; }

	public LocationGridTile gridTileLocation => GetLocationGridTileByXy(Mathf.FloorToInt(localPos.x), Mathf.FloorToInt(localPos.y));

	public Vector3 worldPos { get; private set; }

	public Vector3 localPos { get; private set; }

	protected Rigidbody2D rigidBody => _rigidbody;

	public override void Initialize(TileObject obj)
	{
		base.Initialize(obj);
		_mapLocation = obj.gridTileLocation.parentMap.region;
	}

	public override void PlaceObjectAt(LocationGridTile tile)
	{
		base.PlaceObjectAt(tile);
		localPos = base.transform.localPosition;
		worldPos = base.transform.position;
		Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
		Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
	}

	public override void Reset()
	{
		base.Reset();
		_movement?.Kill();
		_movement = null;
		_mapLocation = null;
		base.obj = null;
		Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
		Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
	}

	private LocationGridTile GetLocationGridTileByXy(int x, int y)
	{
		if (Utilities.IsInRange(x, 0, _mapLocation.innerMap.width) && Utilities.IsInRange(y, 0, _mapLocation.innerMap.height))
		{
			return _mapLocation.innerMap.map[x, y];
		}
		return null;
	}

	public override void SetWorldPosition(Vector3 worldPosition)
	{
		base.SetWorldPosition(worldPosition);
		if (GameManager.Instance.gameHasStarted)
		{
			_movement?.Kill();
			_movement = null;
		}
	}

	protected virtual void Update()
	{
		localPos = base.transform.localPosition;
		worldPos = base.transform.position;
	}

	private void OnProgressionSpeedChanged(PROGRESSION_SPEED progression)
	{
		UpdateMovementSpeedGivenProgression(progression);
	}

	protected virtual void OnGamePaused(bool isPaused)
	{
		if (isPaused)
		{
			_movement.Pause();
		}
		else
		{
			_movement.Play();
		}
	}

	public void DoMove(Vector3 p_position, float p_duration)
	{
		_rigidbody.DOKill();
		_rigidbody.DOMove(p_position, p_duration);
	}

	protected void MoveToRandomDirectionInitial(float p_speed, float distance = 50f)
	{
		StartCoroutine(IMoveToRandomDirection(p_speed, distance));
	}

	protected void MoveToRandomDirection(float p_speed, float distance = 50f)
	{
		MoveToRandomDirectionBase(p_speed, distance);
	}

	private IEnumerator IMoveToRandomDirection(float p_speed, float distance = 50f)
	{
		yield return null;
		MoveToRandomDirectionBase(p_speed, distance);
	}

	protected virtual void MoveToRandomDirectionBase(float p_speed, float distance = 50f)
	{
		Vector3 normalized = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
		normalized *= distance;
		normalized += base.transform.position;
		if (_movement != null)
		{
			_movement.Kill();
		}
		_movement = _rigidbody.DOMove(normalized, p_speed).SetSpeedBased(isSpeedBased: true);
		UpdateMovementSpeedGivenProgression(GameManager.Instance.currProgressionSpeed);
		OnGamePaused(GameManager.Instance.isPaused);
	}

	protected void MoveToCursor(Vector3 p_mouseWorldPosition, float p_speed, float distance = 50f)
	{
		Vector3 normalized = (p_mouseWorldPosition - worldPos).normalized;
		normalized *= distance;
		normalized += base.transform.position;
		if (_movement != null)
		{
			_movement.Kill();
		}
		_movement = _rigidbody.DOMove(normalized, p_speed).SetSpeedBased(isSpeedBased: true);
		UpdateMovementSpeedGivenProgression(GameManager.Instance.currProgressionSpeed);
		OnGamePaused(GameManager.Instance.isPaused);
	}

	protected void MoveToDirection(Vector3 p_targetPos, float p_speed, float distance = 50f)
	{
		Vector3 zero = Vector3.zero;
		zero.x = p_targetPos.x - base.transform.position.x;
		zero.y = p_targetPos.y - base.transform.position.y;
		zero = zero.normalized * distance;
		zero += base.transform.position;
		if (_movement != null)
		{
			_movement.Kill();
		}
		_movement = _rigidbody.DOMove(zero, p_speed).SetSpeedBased(isSpeedBased: true);
		UpdateMovementSpeedGivenProgression(GameManager.Instance.currProgressionSpeed);
		OnGamePaused(GameManager.Instance.isPaused);
	}

	protected virtual void UpdateMovementSpeedGivenProgression(PROGRESSION_SPEED p_progression)
	{
		if (_movement != null)
		{
			switch (p_progression)
			{
			case PROGRESSION_SPEED.X1:
				_movement.timeScale = 1f;
				break;
			case PROGRESSION_SPEED.X2:
				_movement.timeScale = 1.5f;
				break;
			case PROGRESSION_SPEED.X4:
				_movement.timeScale = 2.6f;
				break;
			}
		}
	}
}
