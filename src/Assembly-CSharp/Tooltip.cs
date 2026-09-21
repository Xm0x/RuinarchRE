using System.Collections;
using Ruinarch;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class Tooltip : MonoBehaviour
{
	public static Tooltip Instance;

	public RectTransform mainRT;

	public Canvas canvas;

	public GameObject smallInfoGO;

	public RectTransform smallInfoRT;

	public HorizontalLayoutGroup smallInfoBGParentLG;

	public VerticalLayoutGroup smallInfoVerticalLG;

	public RectTransform smallInfoBGRT;

	public RuinarchText smallInfoLbl;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void ShowSmallInfo(string info, string header = "", bool autoReplaceText = true)
	{
		string text = string.Empty;
		if (!string.IsNullOrEmpty(header))
		{
			text = "<b><size=18>" + header + "</b>\n";
		}
		text = text + "<line-height=70%><size=16>" + info;
		text = text.Replace("\\n", "\n");
		if (autoReplaceText)
		{
			smallInfoLbl.SetTextAndReplaceWithIcons(text);
		}
		else
		{
			smallInfoLbl.text = text;
		}
		if (!IsSmallInfoShowing())
		{
			smallInfoGO.transform.SetParent(base.transform);
			smallInfoGO.SetActive(value: true);
		}
		PositionTooltip(smallInfoGO, smallInfoRT, smallInfoBGRT);
	}

	public void ShowSmallInfo(string info, UIHoverPosition pos, string header = "", bool autoReplaceText = true)
	{
		string text = string.Empty;
		if (!string.IsNullOrEmpty(header))
		{
			text = "<b><size=18>" + header + "</b>\n";
		}
		text = text + "<line-height=70%><size=16>" + info;
		text = text.Replace("\\n", "\n");
		if (autoReplaceText)
		{
			smallInfoLbl.SetTextAndReplaceWithIcons(text);
		}
		else
		{
			smallInfoLbl.text = text;
		}
		PositionTooltip(pos, smallInfoGO, smallInfoRT);
		if (!IsSmallInfoShowing())
		{
			smallInfoGO.SetActive(value: true);
		}
	}

	private IEnumerator ReLayout(LayoutGroup layoutGroup)
	{
		layoutGroup.enabled = false;
		yield return null;
		layoutGroup.enabled = true;
	}

	private void PositionTooltip(GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT)
	{
		PositionTooltip(InputManager.Instance.mousePosition, tooltipParent, rtToReposition, boundsRT);
	}

	private void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT)
	{
		Vector3 newPos = position;
		if (tooltipParent.transform.parent != mainRT)
		{
			tooltipParent.transform.SetParent(mainRT);
		}
		if (tooltipParent.transform.localScale != Vector3.one)
		{
			tooltipParent.transform.localScale = Vector3.one;
		}
		rtToReposition.pivot = new Vector2(0f, 1f);
		RectTransform obj = tooltipParent.transform as RectTransform;
		obj.pivot = new Vector2(0f, 0f);
		Utilities.GetAnchorMinMax(TextAnchor.LowerLeft, out var anchorMin, out var anchorMax);
		obj.anchorMin = anchorMin;
		obj.anchorMax = anchorMax;
		smallInfoBGParentLG.childAlignment = TextAnchor.UpperLeft;
		if (InputManager.Instance != null)
		{
			if (InputManager.Instance.currentCursorType == Cursor_Type.Cross || InputManager.Instance.currentCursorType == Cursor_Type.Check || InputManager.Instance.currentCursorType == Cursor_Type.Link)
			{
				newPos.x += 100f;
				newPos.y -= 32f;
			}
			else
			{
				newPos.x += 25f;
				newPos.y -= 25f;
			}
		}
		Vector3 vector = KeepFullyOnScreen(smallInfoBGRT, newPos, canvas, mainRT);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mainRT, vector, null, out var localPoint);
		(tooltipParent.transform as RectTransform).localPosition = localPoint;
	}

	private Vector3 KeepFullyOnScreen(RectTransform rect, Vector3 newPos, Canvas canvas, RectTransform CanvasRect)
	{
		float min = 0f;
		float scaleFactor = canvas.scaleFactor;
		float max = CanvasRect.sizeDelta.x * scaleFactor - rect.sizeDelta.x * scaleFactor;
		float min2 = rect.sizeDelta.y * scaleFactor;
		float max2 = CanvasRect.sizeDelta.y * scaleFactor;
		newPos.x = Mathf.Clamp(newPos.x, min, max);
		newPos.y = Mathf.Clamp(newPos.y, min2, max2);
		return newPos;
	}

	private void PositionTooltip(UIHoverPosition position, GameObject tooltipParent, RectTransform rt)
	{
		tooltipParent.transform.SetParent(position.transform);
		RectTransform obj = tooltipParent.transform as RectTransform;
		obj.pivot = position.pivot;
		Utilities.GetAnchorMinMax(position.anchor, out var anchorMin, out var anchorMax);
		obj.anchorMin = anchorMin;
		obj.anchorMax = anchorMax;
		obj.anchoredPosition = Vector2.zero;
		smallInfoBGParentLG.childAlignment = position.anchor;
		rt.pivot = position.pivot;
	}

	public bool IsSmallInfoShowing()
	{
		if (smallInfoGO != null)
		{
			return smallInfoGO.activeSelf;
		}
		return false;
	}

	public void HideSmallInfo()
	{
		if (IsSmallInfoShowing())
		{
			smallInfoGO.SetActive(value: false);
		}
	}
}
