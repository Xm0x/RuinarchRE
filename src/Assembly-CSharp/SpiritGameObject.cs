using System;
using System.Collections;
using DG.Tweening;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class SpiritGameObject : MapObjectVisual<TileObject>
{
	[SerializeField]
	private Rigidbody2D _rigidbody;

	private Func<bool> _isMenuShowing;

	private Tweener _movement;

	public bool isRoaming { get; private set; }

	private bool _isPossessing => GetPossessionTarget() != null;

	public override void Initialize(TileObject tileObject)
	{
		base.Initialize(tileObject);
		base.name = tileObject.ToString();
		bool isCorrupted = tileObject.gridTileLocation.corruptionComponent.isCorrupted;
		SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, tileObject.state, tileObject.gridTileLocation.mainBiomeType, isCorrupted));
		visionTrigger = base.transform.GetComponentInChildren<TileObjectVisionTrigger>();
		_isMenuShowing = () => IsMenuShowing(tileObject);
		UpdateSortingOrders(tileObject);
		AkSoundEngine.PostEvent("Play_Spirit_Cast", base.gameObject);
	}

	public override void UpdateSortingOrders(TileObject obj)
	{
		if (objectVisual != null)
		{
			objectVisual.sortingLayerName = "Area Maps";
			objectVisual.sortingOrder = 82;
		}
	}

	public override void UpdateTileObjectVisual(TileObject tileObject)
	{
		SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, tileObject.state, tileObject.gridTileLocation.mainBiomeType, tileObject.gridTileLocation?.corruptionComponent.isCorrupted ?? false));
	}

	private bool IsMenuShowing(TileObject obj)
	{
		if (UIManager.Instance.tileObjectInfoUI.isShowing)
		{
			return UIManager.Instance.tileObjectInfoUI.activeTileObject == obj;
		}
		return false;
	}

	public virtual bool IsMapObjectMenuVisible()
	{
		return _isMenuShowing();
	}

	protected override void OnPointerLeftClick(TileObject poi)
	{
		base.OnPointerLeftClick(poi);
		UIManager.Instance.ShowTileObjectInfo(poi);
	}

	protected override void OnPointerRightClick(TileObject poi)
	{
		base.OnPointerRightClick(poi);
		UIManager.Instance.ShowPlayerActionContextMenu(poi, poi.worldPosition, p_isScreenPosition: false);
	}

	protected override void OnPointerMiddleClick(TileObject poi)
	{
		base.OnPointerMiddleClick(poi);
		_ = UIManager.Instance.characterInfoUI.activeCharacter ?? UIManager.Instance.monsterInfoUI.activeMonster;
	}

	protected override void OnPointerEnter(TileObject character)
	{
		if (character.mapObjectState != MAP_OBJECT_STATE.UNBUILT)
		{
			base.OnPointerEnter(character);
			InnerMapManager.Instance.SetCurrentlyHoveredPOI(character);
			InnerMapManager.Instance.ShowTileData(character.gridTileLocation);
		}
	}

	protected override void OnPointerExit(TileObject poi)
	{
		if (poi.mapObjectState != MAP_OBJECT_STATE.UNBUILT)
		{
			base.OnPointerExit(poi);
			if (InnerMapManager.Instance.currentlyHoveredPoi == poi)
			{
				InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
			}
			UIManager.Instance.HideSmallInfo();
		}
	}

	public void SetIsRoaming(bool state, bool isInitial = false)
	{
		if (isRoaming == state)
		{
			return;
		}
		isRoaming = state;
		if (isRoaming)
		{
			if (isInitial)
			{
				StartCoroutine(IMoveToRandomDirection());
			}
			else
			{
				MoveToRandomDirection();
			}
		}
		else
		{
			StopMovement();
		}
	}

	public LocationGridTile GetLocationGridTileByXy(int x, int y)
	{
		Region region = GridMap.Instance?.mainRegion;
		if (region != null && Utilities.IsInRange(x, 0, region.innerMap.width) && Utilities.IsInRange(y, 0, region.innerMap.height))
		{
			return region.innerMap.map[x, y];
		}
		return null;
	}

	private void Update()
	{
		if (isRoaming && base.gameObject.activeSelf && !GameManager.Instance.isPaused && GameManager.Instance.gameHasStarted)
		{
			LocationGridTile locationGridTileByXy = GetLocationGridTileByXy(Mathf.FloorToInt(base.transform.localPosition.x), Mathf.FloorToInt(base.transform.localPosition.y));
			if (locationGridTileByXy != null)
			{
				base.obj.SetGridTileLocation(locationGridTileByXy);
			}
			else
			{
				Dissipate(base.obj.previousTile);
			}
		}
	}

	private IEnumerator IMoveToRandomDirection()
	{
		yield return null;
		MoveToRandomDirection();
	}

	private void MoveToRandomDirection()
	{
		if (isRoaming && !GameManager.Instance.isPaused)
		{
			Vector3 normalized = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f).normalized;
			normalized *= 3f;
			normalized += base.transform.position;
			LookAt(normalized);
			StopMovement();
			_movement = _rigidbody.DOMove(normalized, 5f).SetSpeedBased(isSpeedBased: true).OnComplete(MoveToRandomDirection);
			UpdateMovementSpeedGivenProgression(GameManager.Instance.currProgressionSpeed);
		}
	}

	public void StartInitialPossessTarget(Character target = null)
	{
		if (isRoaming)
		{
			SetIsRoaming(state: false);
		}
		if (target == null)
		{
			target = GetPossessionTarget();
		}
		if (target != null && target.hasMarker)
		{
			Vector3 position = target.marker.transform.position;
			LookAt(position);
			_movement = _rigidbody.DOMove(position, 10f).SetSpeedBased(isSpeedBased: true).OnComplete(PossessTargetSubsequent);
		}
	}

	private void PossessTargetSubsequent()
	{
		if (_isPossessing)
		{
			Character possessionTarget = GetPossessionTarget();
			if (!possessionTarget.marker.IsNear(base.gameObject.transform.position))
			{
				StartInitialPossessTarget(possessionTarget);
			}
			else
			{
				FinishPossession();
			}
		}
	}

	private Character GetPossessionTarget()
	{
		if (base.obj is RavenousSpirit ravenousSpirit)
		{
			return ravenousSpirit.possessionTarget;
		}
		if (base.obj is FeebleSpirit feebleSpirit)
		{
			return feebleSpirit.possessionTarget;
		}
		if (base.obj is ForlornSpirit forlornSpirit)
		{
			return forlornSpirit.possessionTarget;
		}
		return null;
	}

	private void FinishPossession()
	{
		if (base.obj is RavenousSpirit ravenousSpirit)
		{
			ravenousSpirit.FinishPossession();
		}
		else if (base.obj is FeebleSpirit feebleSpirit)
		{
			feebleSpirit.FinishPossession();
		}
		else if (base.obj is ForlornSpirit forlornSpirit)
		{
			forlornSpirit.FinishPossession();
		}
	}

	private void Dissipate(LocationGridTile p_tile)
	{
		if (base.obj is RavenousSpirit ravenousSpirit)
		{
			ravenousSpirit.Dissipate(p_tile);
		}
		else if (base.obj is FeebleSpirit feebleSpirit)
		{
			feebleSpirit.Dissipate(p_tile);
		}
		else if (base.obj is ForlornSpirit forlornSpirit)
		{
			forlornSpirit.Dissipate(p_tile);
		}
	}

	private void StopMovement()
	{
		if (_movement != null)
		{
			_movement.Kill();
			_movement = null;
		}
	}

	public void UpdateMovementSpeedGivenProgression(PROGRESSION_SPEED p_progression)
	{
	}

	public override void Reset()
	{
		base.Reset();
		isRoaming = false;
		StopMovement();
		base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
	}

	public override void LookAt(Vector3 target, bool force = false)
	{
		Vector3 vector = target - base.transform.position;
		vector.Normalize();
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		Rotate(Quaternion.Euler(0f, 0f, num - 90f), force);
	}

	public override void Rotate(Quaternion target, bool force = false)
	{
		base.transform.rotation = target;
	}
}
