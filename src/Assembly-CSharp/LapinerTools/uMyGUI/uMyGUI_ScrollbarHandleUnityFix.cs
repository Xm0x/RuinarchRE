using UnityEngine;

namespace LapinerTools.uMyGUI;

public class uMyGUI_ScrollbarHandleUnityFix : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_anchorMin = new Vector2(0.8f, 0f);

	[SerializeField]
	private Vector2 m_anchorMax = new Vector2(1f, 1f);

	[SerializeField]
	private Vector2 m_pivot = new Vector2(0.5f, 0.5f);

	[SerializeField]
	private Vector2 m_offsetMin = new Vector2(-10f, -10f);

	[SerializeField]
	private Vector2 m_offsetMax = new Vector2(10f, 10f);

	public void Awake()
	{
		RectTransform component = GetComponent<RectTransform>();
		component.localPosition = Vector3.zero;
		component.anchoredPosition3D = Vector3.zero;
		component.anchorMin = m_anchorMin;
		component.anchorMax = m_anchorMax;
		component.pivot = m_pivot;
		component.offsetMin = m_offsetMin;
		component.offsetMax = m_offsetMax;
	}
}
