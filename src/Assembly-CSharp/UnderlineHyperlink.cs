using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnderlineHyperlink : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	protected bool isHovering;

	protected bool hasBeenUnderlined;

	protected TextMeshProUGUI tmPro;

	protected int linkID;

	private void OnEnable()
	{
		tmPro = GetComponent<TextMeshProUGUI>();
		hasBeenUnderlined = false;
	}

	private void OnDisable()
	{
		isHovering = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (tmPro != null)
		{
			isHovering = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (tmPro != null)
		{
			isHovering = false;
		}
	}

	private void Update()
	{
		if (isHovering)
		{
			UnderlineTextWithObject();
		}
	}

	private void UnderlineTextWithObject()
	{
		linkID = TMP_TextUtilities.FindIntersectingLink(tmPro, Input.mousePosition, null);
		if (linkID != -1)
		{
			hasBeenUnderlined = true;
			TMP_LinkInfo tMP_LinkInfo = tmPro.textInfo.linkInfo[linkID];
			for (int i = 0; i < tMP_LinkInfo.linkTextLength; i++)
			{
				tmPro.textInfo.characterInfo[tMP_LinkInfo.linkTextfirstCharacterIndex + i].style = FontStyles.Underline;
				Debug.LogWarning($"CHAR: {tmPro.textInfo.characterInfo[tMP_LinkInfo.linkTextfirstCharacterIndex + i].character}");
				Debug.LogWarning($"STYLE: {tmPro.textInfo.characterInfo[tMP_LinkInfo.linkTextfirstCharacterIndex + i].style}");
			}
		}
	}

	private void RemoveUnderline()
	{
		hasBeenUnderlined = false;
		if (linkID != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = tmPro.textInfo.linkInfo[linkID];
			int linkTextfirstCharacterIndex = tMP_LinkInfo.linkTextfirstCharacterIndex;
			int startIndex = tMP_LinkInfo.linkTextfirstCharacterIndex + tMP_LinkInfo.linkTextLength;
			string text = tmPro.text.Remove(startIndex, 4);
			text = text.Remove(linkTextfirstCharacterIndex - 3, 3);
			tmPro.text = text;
		}
	}
}
