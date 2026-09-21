using System;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class EventLabel : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private bool allowClickAction = true;

	[SerializeField]
	private EventLabelHoverAction hoverAction;

	[SerializeField]
	private UnityEvent hoverOutAction;

	private Log log;

	private int lastHoveredLinkIndex = -1;

	private bool isHighlighting;

	private bool _shouldColorHighlight = true;

	public Func<object, bool> shouldBeHighlightedChecker;

	public Action<object> onLeftClickAction;

	public Action<object> onRightClickAction;

	[SerializeField]
	protected bool isHovering;

	[SerializeField]
	private bool wasHoveringPreviousFrame;

	private static char[] linkTextSeparators = new char[1] { '|' };

	private Color32 originalColor;

	private void Awake()
	{
		if (text == null)
		{
			text = base.gameObject.GetComponent<TextMeshProUGUI>();
		}
	}

	private void Update()
	{
		wasHoveringPreviousFrame = isHovering;
		if (isHovering)
		{
			HoveringAction();
		}
		else if (wasHoveringPreviousFrame)
		{
			OnPointerExit(null);
		}
	}

	private void OnDisable()
	{
		ResetHighlightValues();
		wasHoveringPreviousFrame = false;
	}

	public void ResetHighlightValues()
	{
		if (lastHoveredLinkIndex != -1 && isHighlighting)
		{
			UnhighlightLink(text.textInfo.linkInfo[lastHoveredLinkIndex]);
		}
		lastHoveredLinkIndex = -1;
		isHighlighting = false;
	}

	public void SetTextMeshPro(TextMeshProUGUI p_textMesh)
	{
		text = p_textMesh;
	}

	public void SetAllowClickAction(bool p_state)
	{
		allowClickAction = p_state;
	}

	public void AddHoverEnterAction(UnityAction<object> p_action)
	{
		hoverAction.AddListener(p_action);
	}

	public void AddHoverExitAction(UnityAction p_action)
	{
		hoverOutAction.AddListener(p_action);
	}

	public void RemoveHoverEnterAction(UnityAction<object> p_action)
	{
		hoverAction.RemoveListener(p_action);
	}

	public void RemoveHoverExitAction(UnityAction p_action)
	{
		hoverOutAction.RemoveListener(p_action);
	}

	public bool HasHoverEnterAction(string p_methodName)
	{
		int persistentEventCount = hoverAction.GetPersistentEventCount();
		for (int i = 0; i < persistentEventCount; i++)
		{
			if (hoverAction.GetPersistentMethodName(i) == p_methodName)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHoverExitAction(string p_methodName)
	{
		int persistentEventCount = hoverOutAction.GetPersistentEventCount();
		for (int i = 0; i < persistentEventCount; i++)
		{
			if (hoverOutAction.GetPersistentMethodName(i) == p_methodName)
			{
				return true;
			}
		}
		return false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!allowClickAction)
		{
			return;
		}
		int num = TMP_TextUtilities.FindIntersectingLink(this.text, InputManager.Instance.mousePosition, null);
		if (num == -1)
		{
			return;
		}
		TMP_LinkInfo tMP_LinkInfo = this.text.textInfo.linkInfo[num];
		object obj = null;
		string linkID = tMP_LinkInfo.GetLinkID();
		if (int.TryParse(linkID, out var _))
		{
			obj = linkID;
		}
		else if (linkID.Contains("|"))
		{
			string[] array = linkID.Split(linkTextSeparators);
			if (array.Length == 2)
			{
				Type type = Type.GetType(array[0]);
				string text = array[1];
				if (type != null && !string.IsNullOrEmpty(text))
				{
					obj = DatabaseManager.Instance.GetObjectFromDatabase(type, text);
				}
			}
		}
		else
		{
			obj = linkID;
		}
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (onLeftClickAction != null)
			{
				if (obj != null)
				{
					onLeftClickAction(obj);
					Messenger.Broadcast(UISignals.EVENT_LABEL_LINK_CLICKED, this);
				}
			}
			else if (obj != null)
			{
				UIManager.Instance.OpenObjectUI(obj);
			}
		}
		else if (eventData.button == PointerEventData.InputButton.Right && obj != null)
		{
			onRightClickAction?.Invoke(obj);
		}
		ResetHighlightValues();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (allowClickAction)
		{
			isHovering = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (allowClickAction)
		{
			isHovering = false;
			HoverOutAction();
		}
	}

	public void SetHighlightChecker(Func<object, bool> shouldBeHighlightedChecker)
	{
		this.shouldBeHighlightedChecker = shouldBeHighlightedChecker;
	}

	public void SetShouldColorHighlight(bool state)
	{
		_shouldColorHighlight = state;
	}

	private bool ShouldBeHighlighted(object obj)
	{
		if (shouldBeHighlightedChecker != null)
		{
			return shouldBeHighlightedChecker(obj);
		}
		return true;
	}

	private void HoveringAction()
	{
		bool flag = true;
		int num = TMP_TextUtilities.FindIntersectingLink(this.text, InputManager.Instance.mousePosition, null);
		if (lastHoveredLinkIndex != -1 && lastHoveredLinkIndex != num && isHighlighting)
		{
			UnhighlightLink(this.text.textInfo.linkInfo[lastHoveredLinkIndex]);
		}
		if (num != -1)
		{
			TMP_LinkInfo linkInfo = this.text.textInfo.linkInfo[num];
			string linkID = linkInfo.GetLinkID();
			object obj = null;
			if (int.TryParse(linkID, out var _))
			{
				obj = linkID;
			}
			else if (linkID.Contains("|"))
			{
				string[] array = linkID.Split(linkTextSeparators);
				if (array.Length == 2)
				{
					Type type = Type.GetType(array[0]);
					string text = array[1];
					if (type != null && !string.IsNullOrEmpty(text))
					{
						obj = DatabaseManager.Instance.GetObjectFromDatabase(type, text);
					}
				}
			}
			else
			{
				obj = linkID;
			}
			if (obj != null)
			{
				if (ShouldBeHighlighted(obj))
				{
					if (lastHoveredLinkIndex != num)
					{
						HighlightLink(linkInfo);
						isHighlighting = true;
					}
				}
				else
				{
					isHighlighting = false;
				}
				hoverAction?.Invoke(obj);
				flag = false;
			}
		}
		lastHoveredLinkIndex = num;
		if (hoverOutAction != null && flag)
		{
			hoverOutAction?.Invoke();
		}
	}

	private void HighlightLink(TMP_LinkInfo linkInfo)
	{
		if (_shouldColorHighlight)
		{
			for (int i = 0; i < linkInfo.linkTextLength; i++)
			{
				int num = linkInfo.linkTextfirstCharacterIndex + i;
				TMP_CharacterInfo tMP_CharacterInfo = text.textInfo.characterInfo[num];
				int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
				int vertexIndex = tMP_CharacterInfo.vertexIndex;
				if (!char.IsWhiteSpace(tMP_CharacterInfo.character))
				{
					Color32[] colors = text.textInfo.meshInfo[materialReferenceIndex].colors32;
					if (colors.Length > vertexIndex + 3)
					{
						originalColor = colors[vertexIndex];
						colors[vertexIndex] = Color.white;
						colors[vertexIndex + 1] = Color.white;
						colors[vertexIndex + 2] = Color.white;
						colors[vertexIndex + 3] = Color.white;
					}
				}
			}
			text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
		}
		InputManager.Instance?.SetCursorTo(Cursor_Type.Link);
	}

	private void UnhighlightLink(TMP_LinkInfo linkInfo)
	{
		if (_shouldColorHighlight)
		{
			for (int i = 0; i < linkInfo.linkTextLength; i++)
			{
				int num = linkInfo.linkTextfirstCharacterIndex + i;
				if (!text.textInfo.characterInfo.IsIndexInArray(num))
				{
					continue;
				}
				TMP_CharacterInfo tMP_CharacterInfo = text.textInfo.characterInfo[num];
				int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
				int vertexIndex = tMP_CharacterInfo.vertexIndex;
				if (char.IsWhiteSpace(tMP_CharacterInfo.character))
				{
					continue;
				}
				Color32[] colors = text.textInfo.meshInfo[materialReferenceIndex].colors32;
				if (colors.Length > vertexIndex + 3)
				{
					Color32 color = originalColor;
					if (colors.IsIndexInArray(vertexIndex))
					{
						colors[vertexIndex] = color;
					}
					if (colors.IsIndexInArray(vertexIndex + 1))
					{
						colors[vertexIndex + 1] = color;
					}
					if (colors.IsIndexInArray(vertexIndex + 2))
					{
						colors[vertexIndex + 2] = color;
					}
					if (colors.IsIndexInArray(vertexIndex + 3))
					{
						colors[vertexIndex + 3] = color;
					}
				}
			}
			text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
		}
		InputManager.Instance?.RevertToPreviousCursor();
	}

	public void HoverOutAction()
	{
		TMP_TextUtilities.FindIntersectingLink(text, InputManager.Instance.mousePosition, null);
		hoverOutAction?.Invoke();
		ResetHighlightValues();
	}

	public void SetOnLeftClickAction(Action<object> onClickAction)
	{
		onLeftClickAction = onClickAction;
	}

	public void SetOnRightClickAction(Action<object> onRightClickAction)
	{
		this.onRightClickAction = onRightClickAction;
	}
}
