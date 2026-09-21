using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	private TextMeshProUGUI _tmpPro;

	private int lastHoveredLinkIndex;

	private bool isHovering;

	private void Start()
	{
		_tmpPro = base.gameObject.GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if (isHovering)
		{
			if (TMP_TextUtilities.FindIntersectingLink(_tmpPro, Input.mousePosition, null) != -1)
			{
				InputManager.Instance.SetCursorTo(Cursor_Type.Link);
			}
			else
			{
				InputManager.Instance.SetCursorTo(Cursor_Type.Default);
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(_tmpPro, Input.mousePosition, null);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = _tmpPro.textInfo.linkInfo[num];
			Debug.Log(tMP_LinkInfo.GetLinkID());
			Application.OpenURL(tMP_LinkInfo.GetLinkID());
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isHovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHovering = false;
	}

	private Color32 SetLinkToColor(TMP_LinkInfo linkInfo, Color32 color)
	{
		Color32 color2 = Color.white;
		for (int i = 0; i < linkInfo.linkTextLength; i++)
		{
			int num = linkInfo.linkTextfirstCharacterIndex + i;
			TMP_CharacterInfo tMP_CharacterInfo = _tmpPro.textInfo.characterInfo[num];
			int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
			int vertexIndex = tMP_CharacterInfo.vertexIndex;
			Color32[] colors = _tmpPro.textInfo.meshInfo[materialReferenceIndex].colors32;
			if (color2 == Color.white)
			{
				color2 = colors[0];
			}
			if (tMP_CharacterInfo.isVisible)
			{
				colors[vertexIndex] = color;
				colors[vertexIndex + 1] = color;
				colors[vertexIndex + 2] = color;
				colors[vertexIndex + 3] = color;
			}
		}
		_tmpPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
		return color2;
	}
}
