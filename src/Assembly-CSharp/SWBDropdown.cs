using System;
using System.Collections;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SWBDropdown : MonoBehaviour
{
	[SerializeField]
	private RuinarchButton m_button;

	[SerializeField]
	private RuinarchText m_text;

	[SerializeField]
	private RectTransform m_entriesRoot;

	[SerializeField]
	private RectTransform m_entriesBG;

	[SerializeField]
	private Scrollbar m_entriesScrollbar;

	[SerializeField]
	private RuinarchButton m_entryButton;

	[SerializeField]
	private int m_entrySpacing = 5;

	[SerializeField]
	private string m_nothingSelectedText = "";

	[SerializeField]
	protected bool m_improveNavigationFocus;

	[SerializeField]
	private string[] m_entries = new string[0];

	[SerializeField]
	private int m_selectedIndex = -1;

	public string[] Entries
	{
		get
		{
			return m_entries;
		}
		set
		{
			m_entries = value;
			HideEntries();
		}
	}

	public int SelectedIndex
	{
		get
		{
			return m_selectedIndex;
		}
		set
		{
			m_selectedIndex = Mathf.Clamp(value, -1, m_entries.Length - 1);
		}
	}

	public event Action<int> OnSelected;

	public void Select(int p_selectedIndex)
	{
		int num = Mathf.Clamp(p_selectedIndex, -1, m_entries.Length - 1);
		bool num2 = num != m_selectedIndex;
		m_selectedIndex = num;
		UpdateText();
		if (num2 && this.OnSelected != null)
		{
			this.OnSelected(m_selectedIndex);
		}
	}

	private void Start()
	{
		if (m_text != null)
		{
			UpdateText();
		}
		else
		{
			Debug.LogError("SWBDropdown: m_text must be set in the inspector!");
		}
		if (m_button != null)
		{
			m_button.onClick.AddListener(OnClick);
		}
		else
		{
			Debug.LogError("SWBDropdown: m_button must be set in the inspector!");
		}
		if (m_entriesRoot != null && m_entriesBG != null)
		{
			HideEntries();
		}
		else
		{
			Debug.LogError("SWBDropdown: m_entriesRoot and m_entriesBG must be set in the inspector!");
		}
	}

	private void LateUpdate()
	{
		if (!m_improveNavigationFocus)
		{
			return;
		}
		EventSystem current = EventSystem.current;
		if (!(current != null) || (!(current.currentSelectedGameObject == null) && current.currentSelectedGameObject.activeInHierarchy))
		{
			return;
		}
		if (current.lastSelectedGameObject != null && current.lastSelectedGameObject.activeInHierarchy)
		{
			current.SetSelectedGameObject(current.lastSelectedGameObject);
		}
		else if (m_entriesRoot != null)
		{
			if (m_entriesRoot.gameObject.activeSelf && m_entriesRoot.childCount > 0 && m_entriesRoot.GetChild(0).GetComponentInChildren<Button>() != null)
			{
				m_entriesRoot.GetChild(0).GetComponentInChildren<Button>().Select();
			}
			else if (m_button != null)
			{
				m_button.Select();
			}
		}
	}

	private void OnClick()
	{
		if (m_entriesRoot != null)
		{
			if (m_entriesRoot.gameObject.activeSelf)
			{
				HideEntries();
			}
			else
			{
				ShowEntries();
			}
		}
	}

	private void ShowEntries()
	{
		if (m_entriesRoot != null && m_entriesBG != null && m_entryButton != null)
		{
			m_entriesRoot.gameObject.SetActive(value: true);
			ClearEntries();
			float size = (GetHeight(m_entryButton) + (float)m_entrySpacing) * (float)m_entries.Length + (float)m_entrySpacing;
			m_entriesBG.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			RectTransform component = m_entryButton.GetComponent<RectTransform>();
			SetText(m_entryButton, m_entries[0]);
			SetOnClick(m_entryButton, 0);
			m_entryButton.interactable = m_selectedIndex != 0;
			for (int i = 1; i < m_entries.Length; i++)
			{
				RuinarchButton ruinarchButton = UnityEngine.Object.Instantiate(m_entryButton);
				ruinarchButton.interactable = i != m_selectedIndex;
				RectTransform component2 = ruinarchButton.GetComponent<RectTransform>();
				component2.SetParent(component.parent, worldPositionStays: true);
				component2.localScale = component.localScale;
				component2.offsetMin = component.offsetMin;
				component2.offsetMax = component.offsetMax;
				component2.localPosition = component.localPosition + Vector3.down * i * (GetHeight(m_entryButton) + (float)m_entrySpacing);
				SetText(ruinarchButton, m_entries[i]);
				SetOnClick(ruinarchButton, i);
			}
			if (m_entriesScrollbar != null)
			{
				StartCoroutine(UpdateScrollBarVisibility());
			}
		}
	}

	private void HideEntries()
	{
		if (m_entriesRoot != null)
		{
			ClearEntries();
			m_entriesRoot.gameObject.SetActive(value: false);
		}
	}

	private void ClearEntries()
	{
		if (!(m_entriesBG != null) || !(m_entryButton != null))
		{
			return;
		}
		for (int i = 0; i < m_entriesBG.childCount; i++)
		{
			if (m_entriesBG.GetChild(i) != m_entryButton.transform)
			{
				UnityEngine.Object.Destroy(m_entriesBG.GetChild(i).gameObject);
			}
		}
	}

	private void UpdateText()
	{
		if (m_text != null)
		{
			bool flag = m_selectedIndex >= 0 && m_selectedIndex < m_entries.Length;
			m_text.text = (flag ? m_entries[m_selectedIndex] : m_nothingSelectedText);
		}
	}

	private void SetOnClick(RuinarchButton p_button, int p_selectedIndex)
	{
		p_button.onClick.RemoveAllListeners();
		p_button.onClick.AddListener(delegate
		{
			Select(p_selectedIndex);
			HideEntries();
		});
	}

	private void SetText(RuinarchButton p_button, string p_text)
	{
		RuinarchText text = p_button.text;
		if (text != null)
		{
			text.text = p_text;
		}
	}

	private float GetHeight(RuinarchButton p_button)
	{
		if (!(p_button != null))
		{
			return 0f;
		}
		return GetHeight(p_button.GetComponent<RectTransform>());
	}

	private float GetHeight(RectTransform p_rTransform)
	{
		if (!(p_rTransform != null))
		{
			return 0f;
		}
		return p_rTransform.rect.yMax - p_rTransform.rect.yMin;
	}

	private IEnumerator UpdateScrollBarVisibility()
	{
		yield return new WaitForEndOfFrame();
		if (m_entriesScrollbar != null)
		{
			m_entriesScrollbar.gameObject.SetActive(m_entriesScrollbar.size < 0.985f);
		}
	}
}
