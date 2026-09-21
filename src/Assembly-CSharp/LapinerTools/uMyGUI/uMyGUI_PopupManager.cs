using System;
using System.Collections.Generic;
using UnityEngine;

namespace LapinerTools.uMyGUI;

public class uMyGUI_PopupManager : MonoBehaviour
{
	public const string POPUP_LOADING = "loading";

	public const string POPUP_TEXT = "text";

	public const string POPUP_DROPDOWN = "dropdown";

	public const string BTN_OK = "ok";

	public const string BTN_YES = "yes";

	public const string BTN_NO = "no";

	private static uMyGUI_PopupManager s_instance;

	[SerializeField]
	private uMyGUI_Popup[] m_popups = new uMyGUI_Popup[0];

	[SerializeField]
	private string[] m_popupNames = new string[0];

	[SerializeField]
	private CanvasGroup[] m_deactivatedElementsWhenPopupIsShown = new CanvasGroup[0];

	public static uMyGUI_PopupManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = UnityEngine.Object.FindObjectOfType<uMyGUI_PopupManager>();
			}
			if (s_instance == null)
			{
				s_instance = new GameObject(typeof(uMyGUI_PopupManager).Name).AddComponent<uMyGUI_PopupManager>();
			}
			return s_instance;
		}
	}

	public static bool IsInstanceSet => s_instance != null;

	public uMyGUI_Popup[] Popups
	{
		get
		{
			return m_popups;
		}
		set
		{
			m_popups = value;
		}
	}

	public string[] PopupNames
	{
		get
		{
			return m_popupNames;
		}
		set
		{
			m_popupNames = value;
		}
	}

	public CanvasGroup[] DeactivatedElementsWhenPopupIsShown
	{
		get
		{
			return m_deactivatedElementsWhenPopupIsShown;
		}
		set
		{
			m_deactivatedElementsWhenPopupIsShown = value;
		}
	}

	public bool IsPopupShown
	{
		get
		{
			for (int i = 0; i < m_popups.Length; i++)
			{
				if (m_popups[i] != null && m_popups[i].IsShown)
				{
					return true;
				}
			}
			return false;
		}
	}

	public uMyGUI_Popup ShowPopup(string p_name)
	{
		for (int i = 0; i < m_popupNames.Length && i < m_popups.Length; i++)
		{
			if (m_popupNames[i] == p_name)
			{
				return ShowPopup(i);
			}
		}
		if (LoadPopupFromResources(p_name) != null)
		{
			return ShowPopup(p_name);
		}
		return null;
	}

	public uMyGUI_Popup HidePopup(string p_name)
	{
		for (int i = 0; i < m_popupNames.Length && i < m_popups.Length; i++)
		{
			if (m_popupNames[i] == p_name)
			{
				return HidePopup(i);
			}
		}
		return null;
	}

	public uMyGUI_Popup ShowPopup(int p_index)
	{
		if (p_index >= 0 && p_index < m_popups.Length)
		{
			m_popups[p_index].Show();
			return m_popups[p_index];
		}
		Debug.LogError("uMyGUI_PopupManager: ShowPopup: popup index '" + p_index + "' is out of bounds [0," + m_popups.Length + "]!");
		return null;
	}

	public uMyGUI_Popup HidePopup(int p_index)
	{
		if (p_index >= 0 && p_index < m_popups.Length)
		{
			m_popups[p_index].Hide();
			return m_popups[p_index];
		}
		Debug.LogError("uMyGUI_PopupManager: HidePopup: popup index '" + p_index + "' is out of bounds [0," + m_popups.Length + "]!");
		return null;
	}

	public bool HasPopup(string p_name)
	{
		for (int i = 0; i < m_popupNames.Length; i++)
		{
			if (m_popupNames[i] == p_name)
			{
				return true;
			}
		}
		return false;
	}

	public bool AddPopup(uMyGUI_Popup p_popup, string p_name)
	{
		Canvas canvas = null;
		if (m_popups.Length != 0 && m_popups[0] != null && m_popups[0].transform.parent != null)
		{
			canvas = m_popups[0].transform.parent.GetComponentInParent<Canvas>();
		}
		if (canvas == null)
		{
			canvas = GetComponentInParent<Canvas>();
		}
		if (canvas == null)
		{
			canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
		}
		if (canvas == null)
		{
			Debug.LogError("uMyGUI_PopupManager: AddPopup: there is no Canvas in this level!");
			return false;
		}
		uMyGUI_Popup[] popups = m_popups;
		string[] popupNames = m_popupNames;
		m_popups = new uMyGUI_Popup[m_popups.Length + 1];
		m_popupNames = new string[m_popupNames.Length + 1];
		Array.Copy(popups, m_popups, popups.Length);
		Array.Copy(popupNames, m_popupNames, popupNames.Length);
		m_popups[m_popups.Length - 1] = p_popup;
		m_popupNames[m_popups.Length - 1] = p_name;
		p_popup.transform.SetParent(canvas.transform, worldPositionStays: false);
		HidePopup(m_popups.Length - 1);
		return true;
	}

	public bool RemovePopup(uMyGUI_Popup p_popup)
	{
		for (int i = 0; i < m_popups.Length; i++)
		{
			if (m_popups[i] == p_popup)
			{
				List<uMyGUI_Popup> list = new List<uMyGUI_Popup>(m_popups);
				list.RemoveAt(i);
				m_popups = list.ToArray();
				List<string> list2 = new List<string>(m_popupNames);
				list2.RemoveAt(i);
				m_popupNames = list2.ToArray();
				return true;
			}
		}
		return false;
	}

	private uMyGUI_Popup LoadPopupFromResources(string p_name)
	{
		uMyGUI_Popup uMyGUI_Popup2 = UnityEngine.Object.Instantiate(Resources.Load<uMyGUI_Popup>("popup_" + p_name + "_root"));
		if (uMyGUI_Popup2 != null && AddPopup(uMyGUI_Popup2, p_name))
		{
			return uMyGUI_Popup2;
		}
		return null;
	}

	private void Awake()
	{
		if (m_popups.Length != m_popupNames.Length)
		{
			Debug.LogError("uMyGUI_PopupManager: m_popups and m_popupNames must have the same length (" + m_popups.Length + "!=" + m_popupNames.Length + ")!");
		}
		for (int i = 0; i < m_popups.Length; i++)
		{
			HidePopup(i);
		}
	}

	private void Update()
	{
		bool interactable = !IsPopupShown;
		for (int i = 0; i < m_deactivatedElementsWhenPopupIsShown.Length; i++)
		{
			m_deactivatedElementsWhenPopupIsShown[i].interactable = interactable;
		}
	}
}
