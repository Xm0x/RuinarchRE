using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ruinarch.Custom_UI;

public class RuinarchDropdown : TMP_Dropdown
{
	private Canvas m_mainDropdownCanvas;

	private bool isBlockerActive;

	protected override void Awake()
	{
		base.Awake();
		m_mainDropdownCanvas = GetOrAddComponent<Canvas>(base.gameObject);
	}

	protected override GameObject CreateBlocker(Canvas rootCanvas)
	{
		GameObject obj = base.CreateBlocker(rootCanvas);
		if (obj != null)
		{
			isBlockerActive = true;
			m_mainDropdownCanvas.overrideSorting = true;
			m_mainDropdownCanvas.sortingOrder = 30000;
		}
		return obj;
	}

	protected override void DestroyBlocker(GameObject blocker)
	{
		base.DestroyBlocker(blocker);
		m_mainDropdownCanvas.overrideSorting = false;
		isBlockerActive = false;
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (isBlockerActive)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	private static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (!val)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}
}
