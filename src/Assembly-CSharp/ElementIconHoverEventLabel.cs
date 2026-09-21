using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ElementIconHoverEventLabel : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private TextMeshProUGUI _text;

	[SerializeField]
	protected bool isHovering;

	[SerializeField]
	private bool wasHoveringPreviousFrame;

	private bool _hasHoveredElement;

	private void Awake()
	{
		if (_text == null)
		{
			_text = base.gameObject.GetComponent<TextMeshProUGUI>();
		}
	}

	private void Update()
	{
		wasHoveringPreviousFrame = isHovering;
		if (isHovering)
		{
			HoverEnterAction();
		}
		else if (wasHoveringPreviousFrame)
		{
			OnPointerExit(null);
		}
	}

	private void OnDisable()
	{
		wasHoveringPreviousFrame = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isHovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHovering = false;
		HoverExitAction();
	}

	private void HoverEnterAction()
	{
		_hasHoveredElement = false;
		int num = TMP_TextUtilities.FindIntersectingLink(_text, InputManager.Instance.mousePosition, null);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = _text.textInfo.linkInfo[num];
			string linkID = tMP_LinkInfo.GetLinkID();
			if (linkID.Contains("#"))
			{
				string text = linkID.Remove(linkID.Length - 1);
				string text2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", text);
				if (string.IsNullOrEmpty(text2))
				{
					text2 = text;
				}
				UIManager.Instance.ShowSmallInfo(text2);
				_hasHoveredElement = true;
			}
		}
		if (!_hasHoveredElement)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	public void HoverExitAction()
	{
		if (_hasHoveredElement)
		{
			_hasHoveredElement = false;
			UIManager.Instance.HideSmallInfo();
		}
	}
}
