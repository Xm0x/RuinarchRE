using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class Selector : MonoBehaviour
{
	public static Selector Instance;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private float selectionTiming;

	[SerializeField]
	private Vector2 from;

	[SerializeField]
	private Vector2 to;

	private ISelectable _selected;

	private void Awake()
	{
		Instance = this;
		base.gameObject.SetActive(value: false);
	}

	public void Select(ISelectable selectable, Transform parent = null)
	{
		base.gameObject.SetActive(value: true);
		_selected = selectable;
		Vector3 worldPosition = _selected.worldPosition;
		if (selectable is ManMadeStructure manMadeStructure)
		{
			if (!Utilities.IsEven(manMadeStructure.structureObj.size.x))
			{
				worldPosition.x += 0.5f;
			}
			if (!Utilities.IsEven(manMadeStructure.structureObj.size.y))
			{
				worldPosition.y += 0.5f;
			}
		}
		else if (selectable is NaturalStructureWithStructureObject naturalStructureWithStructureObject)
		{
			if (!Utilities.IsEven(naturalStructureWithStructureObject.structureObj.size.x))
			{
				worldPosition.x += 0.5f;
			}
			if (!Utilities.IsEven(naturalStructureWithStructureObject.structureObj.size.y))
			{
				worldPosition.y += 0.5f;
			}
		}
		base.transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(0f, 0f, 0f));
		Vector2 size = from;
		size.x *= selectable.selectableSize.x;
		size.y *= selectable.selectableSize.y;
		_spriteRenderer.size = size;
		Vector2 endValue = to;
		endValue.x *= selectable.selectableSize.x;
		endValue.y *= selectable.selectableSize.y;
		base.transform.SetParent((parent != null) ? parent : InnerMapManager.Instance.transform);
		DOTween.To(() => _spriteRenderer.size, delegate(Vector2 x)
		{
			_spriteRenderer.size = x;
		}, endValue, selectionTiming);
	}

	public bool IsSelected(ISelectable p_selectable)
	{
		return _selected == p_selectable;
	}

	public void Deselect()
	{
		base.gameObject.SetActive(value: false);
		_selected = null;
	}
}
