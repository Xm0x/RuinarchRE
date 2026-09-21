using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;

public class SWBPagination : MonoBehaviour
{
	[SerializeField]
	private RuinarchButton m_previousButton;

	[SerializeField]
	private RuinarchButton m_nextButton;

	[SerializeField]
	private RuinarchButton m_pageButton;

	[Header("Pooling")]
	[SerializeField]
	private Transform _poolParent;

	private Queue<RuinarchButton> _pooledButtons = new Queue<RuinarchButton>(10);

	[SerializeField]
	private int m_pageCount = 1;

	[SerializeField]
	private int m_maxPageBtnCount = 9;

	[SerializeField]
	private int m_selectedPage;

	private RectTransform m_rectTransform;

	private RectTransform m_pageButtonTransform;

	private int m_offset;

	private List<RuinarchButton> m_pageButtons = new List<RuinarchButton>();

	public int PageCount
	{
		get
		{
			return m_pageCount;
		}
		set
		{
			SetPageCount(value);
		}
	}

	public int MaxPageBtnCount
	{
		get
		{
			return m_maxPageBtnCount;
		}
		set
		{
			m_maxPageBtnCount = value;
			SetPageCount(PageCount);
		}
	}

	public int SelectedPage
	{
		get
		{
			return m_selectedPage;
		}
		set
		{
			SelectPage(value);
		}
	}

	public RectTransform RTransform
	{
		get
		{
			if (!(m_rectTransform != null))
			{
				return m_rectTransform = GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	private RectTransform PageButtonTransform
	{
		get
		{
			if (!(m_pageButtonTransform != null) && !(m_pageButton == null))
			{
				return m_pageButtonTransform = m_pageButton.GetComponent<RectTransform>();
			}
			return m_pageButtonTransform;
		}
	}

	public event Action<int> OnPageSelected;

	public void SetPageCount(int p_newPageCount)
	{
		m_pageCount = Mathf.Max(1, p_newPageCount);
		if (p_newPageCount <= 1)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (m_pageButton != null)
		{
			base.gameObject.SetActive(value: true);
			UpdateUI();
		}
		else
		{
			Debug.LogError("uMyGUI_PageBox: SetPageCount: m_pageButton must be set in the inspector!");
		}
	}

	public void SelectPageAndCenterOffset(int p_selectedPage)
	{
		m_offset = Mathf.Min(m_pageCount - m_maxPageBtnCount, Mathf.Max(0, p_selectedPage - 1 - m_maxPageBtnCount / 2));
		SelectPage(p_selectedPage);
	}

	public void SelectPage(int p_selectedPage)
	{
		int num = Mathf.Clamp(p_selectedPage, 0, m_pageCount);
		bool num2 = num != m_selectedPage;
		m_selectedPage = num;
		UpdateUI();
		if (num2 && this.OnPageSelected != null)
		{
			this.OnPageSelected(p_selectedPage);
		}
	}

	public void UpdateUI()
	{
		Clear();
		int num = Mathf.Min(m_pageCount, m_maxPageBtnCount);
		float size = GetWidth(m_previousButton) + GetWidth(m_nextButton) + GetWidth(m_pageButton) * (float)num;
		RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		int num2 = Mathf.Max(0, m_selectedPage - m_maxPageBtnCount);
		int num3 = Mathf.Max(0, Mathf.Min(m_pageCount - m_maxPageBtnCount, m_selectedPage - 1));
		if (num2 - 1 >= m_offset)
		{
			m_offset = num2;
		}
		else if (num3 + 1 <= m_offset)
		{
			m_offset = num3;
		}
		SetText(m_pageButton, (1 + m_offset).ToString());
		SetOnClick(m_pageButton, 1 + m_offset);
		for (int i = 2; i <= num; i++)
		{
			RuinarchButton ruinarchButton = CreateNewPageButton();
			Transform obj = ruinarchButton.transform;
			obj.SetParent(PageButtonTransform.parent, worldPositionStays: true);
			obj.localScale = PageButtonTransform.localScale;
			obj.localPosition = PageButtonTransform.localPosition + Vector3.right * (i - 1) * GetWidth(m_pageButton);
			SetText(ruinarchButton, (i + m_offset).ToString());
			SetOnClick(ruinarchButton, i + m_offset);
			m_pageButtons.Add(ruinarchButton);
		}
		for (int j = 0; j < m_pageButtons.Count; j++)
		{
			RuinarchButton ruinarchButton2 = m_pageButtons[j];
			int num4 = j + 1 + m_offset;
			ruinarchButton2.enabled = num4 != m_selectedPage;
			if (j == m_selectedPage - 1)
			{
				ruinarchButton2.text.fontStyle = FontStyles.Underline;
			}
		}
		if (m_nextButton != null)
		{
			m_nextButton.GetComponent<RectTransform>().localPosition = PageButtonTransform.localPosition + Vector3.right * num * GetWidth(m_pageButton);
		}
	}

	private void Start()
	{
		SetPageCount(m_pageCount);
		if (m_previousButton != null)
		{
			m_previousButton.onClick.AddListener(delegate
			{
				SelectPageAndCenterOffset(Mathf.Max(1, m_selectedPage - 1));
			});
		}
		if (m_nextButton != null)
		{
			m_nextButton.onClick.AddListener(delegate
			{
				SelectPageAndCenterOffset(m_selectedPage + 1);
			});
		}
	}

	private void SetText(RuinarchButton p_button, string p_text)
	{
		RuinarchText text = p_button.text;
		if (text != null)
		{
			text.fontStyle = FontStyles.Normal;
			text.text = p_text;
		}
	}

	private void SetOnClick(RuinarchButton p_button, int p_pageNumber)
	{
		p_button.onClick.RemoveAllListeners();
		p_button.onClick.AddListener(delegate
		{
			SelectPage(p_pageNumber);
		});
	}

	private float GetWidth(RuinarchButton p_button)
	{
		if (!(p_button != null))
		{
			return 0f;
		}
		return GetWidth(p_button.GetComponent<RectTransform>());
	}

	private float GetWidth(RectTransform p_rTransform)
	{
		if (!(p_rTransform != null))
		{
			return 0f;
		}
		return p_rTransform.rect.xMax - p_rTransform.rect.xMin;
	}

	private void Clear()
	{
		for (int i = 0; i < m_pageButtons.Count; i++)
		{
			if (m_pageButtons[i] != null && m_pageButtons[i] != m_pageButton)
			{
				DestroyButton(m_pageButtons[i]);
			}
		}
		m_pageButtons.Clear();
		m_pageButtons.Add(m_pageButton);
	}

	private RuinarchButton CreateNewPageButton()
	{
		if (_pooledButtons.Count > 0)
		{
			return _pooledButtons.Dequeue();
		}
		return UnityEngine.Object.Instantiate(m_pageButton, _poolParent);
	}

	private void DestroyButton(RuinarchButton p_button)
	{
		p_button.transform.SetParent(_poolParent);
		p_button.transform.localPosition = Vector3.zero;
		_pooledButtons.Enqueue(p_button);
	}
}
