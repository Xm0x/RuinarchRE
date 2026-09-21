using System;
using System.Collections;
using DG.Tweening;
using EZObjectPools;
using Inner_Maps;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UtilityScripts;

public abstract class BaseMapObjectVisual : PooledObject, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	protected SpriteRenderer objectVisual;

	[SerializeField]
	protected Collider2D clickCollider;

	public BaseVisionTrigger visionTrigger;

	[Header("HP")]
	[FormerlySerializedAs("hpBarGO")]
	[SerializeField]
	private GameObject _hpBarGO;

	[FormerlySerializedAs("hpFill")]
	[SerializeField]
	private Image _hpFill;

	public Image aspeedFill;

	public Transform particleEffectParent;

	public Transform statusIconsParent;

	protected Action onHoverOverAction;

	protected Action onHoverExitAction;

	protected Action onLeftClickAction;

	protected Action onRightClickAction;

	protected Action onMiddleClickAction;

	private Vector3 _originalVisualLocalPos;

	public ISelectable selectable { get; protected set; }

	private LocationGridTileGUS graphUpdateScene { get; set; }

	public int usedSpriteIndex { get; private set; }

	public Quaternion rotation { get; private set; }

	public GameObject gameObjectVisual => base.gameObject;

	public Sprite usedSprite
	{
		get
		{
			if (objectVisual != null)
			{
				return objectVisual.sprite;
			}
			return null;
		}
	}

	public SpriteRenderer objectSpriteRenderer => objectVisual;

	public bool hasHPBarGO => _hpBarGO != null;

	public GameObject hpBarGO
	{
		get
		{
			if (_hpBarGO == null)
			{
				_hpBarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("Map Object HP Bar", Vector2.zero, Quaternion.identity, base.transform);
				RectTransform obj = _hpBarGO.transform as RectTransform;
				Vector2 anchoredPosition = new Vector2(0f, -0.2f);
				obj.anchoredPosition = anchoredPosition;
				Image[] componentsInChildren = _hpBarGO.GetComponentsInChildren<Image>();
				foreach (Image image in componentsInChildren)
				{
					if (image.name == "Fill")
					{
						_hpFill = image;
						break;
					}
				}
				_hpBarGO.gameObject.SetActive(value: false);
			}
			return _hpBarGO;
		}
	}

	private void Awake()
	{
		if (objectVisual != null)
		{
			_originalVisualLocalPos = objectVisual.transform.localPosition;
		}
	}

	protected void Initialize(ISelectable selectable)
	{
		this.selectable = selectable;
	}

	public virtual Sprite GetSeizeSprite(IPointOfInterest poi)
	{
		return objectVisual?.sprite;
	}

	public void SetRotation(float rotation)
	{
		if (!(objectVisual == null))
		{
			Quaternion localRotation = Quaternion.Euler(0f, 0f, rotation);
			objectVisual.transform.localRotation = localRotation;
			this.rotation = localRotation;
		}
	}

	public void SetRotation(Quaternion rotation)
	{
		if (!(objectVisual == null))
		{
			objectVisual.transform.localRotation = rotation;
			this.rotation = rotation;
		}
	}

	public virtual void SetVisual(Sprite sprite, int p_spriteIndex = -1)
	{
		if (objectVisual == null)
		{
			return;
		}
		objectVisual.sprite = sprite;
		usedSpriteIndex = p_spriteIndex;
		if (sprite != null && p_spriteIndex == -1 && selectable is TileObject tileObject)
		{
			TileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObject.tileObjectType);
			if (tileObjectScriptableObject != null)
			{
				usedSpriteIndex = tileObjectScriptableObject.GetIndexBySprite(sprite);
			}
		}
	}

	private void SetColor(Color color)
	{
		if (!(objectVisual == null))
		{
			objectVisual.color = color;
		}
	}

	public void SetActiveState(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public virtual void SetVisualAlpha(float alpha)
	{
		if (!(objectVisual == null))
		{
			Color color = objectVisual.color;
			color.a = alpha;
			SetColor(color);
		}
	}

	protected void SetHoverObjectState(bool state)
	{
		if (!(PlayerManager.Instance != null) || PlayerManager.Instance.player == null || !PlayerManager.Instance.player.IsPerformingPlayerAction())
		{
			if (state)
			{
				InnerMapManager.Instance.ShowHoverVisual(this);
			}
			else
			{
				InnerMapManager.Instance.HideHoverVisual();
			}
		}
	}

	public StatusIcon AddStatusIcon(string statusName)
	{
		StatusIcon component = ObjectPoolManager.Instance.InstantiateObjectFromPool(TraitManager.Instance.traitIconPrefab.name, Vector3.zero, Quaternion.identity, statusIconsParent).GetComponent<StatusIcon>();
		component.SetIcon(TraitManager.Instance.GetTraitIcon(statusName));
		return component;
	}

	public virtual void LookAt(Vector3 target, bool force = false)
	{
	}

	public virtual void Rotate(Quaternion target, bool force = false)
	{
	}

	public void SetMaterial(Material material)
	{
		if (!(objectVisual == null))
		{
			objectVisual.material = material;
		}
	}

	public virtual void OnSeizeVisual(IPointOfInterest poi)
	{
	}

	public virtual bool IsInvisibleToPlayer()
	{
		if (objectVisual != null)
		{
			return Mathf.Approximately(objectVisual.color.a, 0f);
		}
		return true;
	}

	public void ExecuteClickAction(PointerEventData.InputButton button)
	{
		switch (button)
		{
		case PointerEventData.InputButton.Left:
			onLeftClickAction?.Invoke();
			break;
		case PointerEventData.InputButton.Right:
			onRightClickAction?.Invoke();
			break;
		case PointerEventData.InputButton.Middle:
			onMiddleClickAction?.Invoke();
			break;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void ExecuteHoverEnterAction()
	{
		onHoverOverAction?.Invoke();
	}

	public void ExecuteHoverExitAction()
	{
		onHoverExitAction?.Invoke();
	}

	public override void Reset()
	{
		base.Reset();
		if (objectVisual != null)
		{
			SetVisualAlpha(1f);
		}
		if ((bool)_hpBarGO)
		{
			HideHPBar();
		}
		if ((bool)visionTrigger)
		{
			visionTrigger.Reset();
		}
		selectable = null;
		DestroyAllStatusIcons();
		DestroyAllParticleEffects();
		SetMaterial(InnerMapManager.Instance.assetManager.defaultObjectMaterial);
		DOTween.Kill(base.transform);
	}

	private void DestroyAllStatusIcons()
	{
		if (!(statusIconsParent != null))
		{
			return;
		}
		Transform[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<Transform>(statusIconsParent.gameObject);
		if (componentsInDirectChildren != null)
		{
			for (int i = 0; i < componentsInDirectChildren.Length; i++)
			{
				ObjectPoolManager.Instance.DestroyObject(componentsInDirectChildren[i].gameObject);
			}
		}
	}

	protected virtual void DestroyAllParticleEffects()
	{
		if (!(particleEffectParent != null))
		{
			return;
		}
		Transform[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<Transform>(particleEffectParent.gameObject);
		if (componentsInDirectChildren != null)
		{
			for (int i = 0; i < componentsInDirectChildren.Length; i++)
			{
				ObjectPoolManager.Instance.DestroyObject(componentsInDirectChildren[i].gameObject);
			}
		}
	}

	public void DestroyParticlesByName(string p_name)
	{
		if (!(particleEffectParent != null))
		{
			return;
		}
		Transform[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<Transform>(particleEffectParent.gameObject);
		if (componentsInDirectChildren == null)
		{
			return;
		}
		for (int i = 0; i < componentsInDirectChildren.Length; i++)
		{
			GameObject gameObject = componentsInDirectChildren[i].gameObject;
			if (gameObject.name == p_name || gameObject.name.Contains(p_name))
			{
				ObjectPoolManager.Instance.DestroyObject(gameObject);
			}
		}
	}

	public override void BeforeDestroyActions()
	{
		base.BeforeDestroyActions();
		DestroyExistingGUS();
	}

	public bool IsTweening()
	{
		return DOTween.IsTweening(base.transform);
	}

	public void TweenTo(Transform _target, float duration, Action _onReachTargetAction)
	{
		Vector3 position = _target.position;
		Tweener tween = base.transform.DOMove(position, duration).SetEase(Ease.Linear).SetAutoKill(autoKillOnCompletion: false)
			.OnComplete(_onReachTargetAction.Invoke);
		tween.OnUpdate(delegate
		{
			tween.ChangeEndValue(_target.position, snapStartValue: true);
		});
		if (GameManager.Instance.isPaused)
		{
			base.transform.DOPause();
		}
		else
		{
			base.transform.DOPlay();
		}
	}

	public void OnReachTarget()
	{
		DOTween.Kill(base.transform);
	}

	public virtual void PlaceObjectAt(LocationGridTile tile)
	{
		Transform obj = base.transform;
		obj.SetParent(tile.parentMap.structureParent);
		Vector3 centeredWorldLocation = tile.centeredWorldLocation;
		obj.position = centeredWorldLocation;
		if (objectVisual != null)
		{
			rotation = objectVisual.transform.localRotation;
			if (objectVisual.gameObject != base.gameObject)
			{
				objectVisual.transform.localPosition = _originalVisualLocalPos;
			}
		}
	}

	public virtual void SetWorldPosition(Vector3 worldPosition)
	{
		base.transform.position = worldPosition;
	}

	public void ShowHPBar(IPointOfInterest poi)
	{
		hpBarGO?.SetActive(value: true);
		UpdateHP(poi);
	}

	public void HideHPBar()
	{
		hpBarGO?.SetActive(value: false);
	}

	public void UpdateHP(IPointOfInterest poi)
	{
		if (hpBarGO != null && hpBarGO.activeSelf && _hpFill != null)
		{
			_hpFill.fillAmount = (float)poi.currentHP / (float)poi.maxHP;
		}
	}

	public void QuickShowHPBar(IPointOfInterest poi)
	{
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(QuickShowHPBarCoroutine(poi));
		}
	}

	private IEnumerator QuickShowHPBarCoroutine(IPointOfInterest poi)
	{
		ShowHPBar(poi);
		yield return GameUtilities.waitFor2Seconds;
		if (!(poi is Character character) || !character.combatComponent.isInCombat)
		{
			HideHPBar();
		}
	}

	public void InitializeGUS(Vector2 offset, Vector2 size, [NotNull] LocationGridTile tile)
	{
		if (graphUpdateScene == null)
		{
			LocationGridTileGUS component = ObjectPoolManager.Instance.InstantiateObjectFromPool("LocationGridTileGUS", Vector3.zero, Quaternion.identity, base.transform).GetComponent<LocationGridTileGUS>();
			graphUpdateScene = component;
		}
		graphUpdateScene.Initialize(offset, size, tile.parentMap);
	}

	public void DestroyExistingGUS()
	{
		if (!(graphUpdateScene == null))
		{
			graphUpdateScene.Destroy();
			graphUpdateScene = null;
		}
	}

	public void ApplyGraphUpdate()
	{
		graphUpdateScene.InstantApply();
	}
}
