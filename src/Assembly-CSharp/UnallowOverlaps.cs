using UnityEngine;

public class UnallowOverlaps : MonoBehaviour
{
	public new OVERLAP_UI_TAG tag;

	private Vector3 _defaultLocalPosition;

	public RectTransform rectTransform { get; private set; }

	public Vector2 anchoredOffsetMin
	{
		get
		{
			Vector2 anchorMin = rectTransform.anchorMin;
			anchorMin.x *= Screen.width;
			anchorMin.y *= Screen.height;
			return anchorMin + rectTransform.offsetMin;
		}
	}

	public Vector2 anchoredOffsetMax
	{
		get
		{
			Vector2 anchorMax = rectTransform.anchorMax;
			anchorMax.x *= Screen.width;
			anchorMax.y *= Screen.height;
			return anchorMax + rectTransform.offsetMax;
		}
	}

	public void Initialize()
	{
		_defaultLocalPosition = rectTransform.anchoredPosition;
	}

	private void OnEnable()
	{
		if (rectTransform == null)
		{
			rectTransform = base.gameObject.GetComponent<RectTransform>();
			UIManager.Instance.AddUnallowOverlapUI(this);
			Initialize();
		}
		UnallowOverlaps overlappedUI = UIManager.Instance.GetOverlappedUI(this);
		if (overlappedUI != null)
		{
			if (overlappedUI.tag == OVERLAP_UI_TAG.Top && tag == OVERLAP_UI_TAG.Bottom)
			{
				Reposition(overlappedUI);
			}
			else if (overlappedUI.tag == OVERLAP_UI_TAG.Bottom && tag == OVERLAP_UI_TAG.Top)
			{
				overlappedUI.Reposition(this);
			}
			return;
		}
		DefaultPosition();
		overlappedUI = UIManager.Instance.GetOverlappedUI(this);
		if (overlappedUI != null)
		{
			if (overlappedUI.tag == OVERLAP_UI_TAG.Top && tag == OVERLAP_UI_TAG.Bottom)
			{
				Reposition(overlappedUI);
			}
			else if (overlappedUI.tag == OVERLAP_UI_TAG.Bottom && tag == OVERLAP_UI_TAG.Top)
			{
				overlappedUI.Reposition(this);
			}
		}
	}

	public void Reposition(UnallowOverlaps overlappedTop)
	{
		float num = overlappedTop.anchoredOffsetMin.x - rectTransform.rect.width;
		if (num < 0f)
		{
			num = overlappedTop.anchoredOffsetMax.x;
		}
		base.transform.localPosition = new Vector3(num, base.transform.localPosition.y, base.transform.localPosition.z);
	}

	private void DefaultPosition()
	{
		rectTransform.anchoredPosition = _defaultLocalPosition;
	}
}
