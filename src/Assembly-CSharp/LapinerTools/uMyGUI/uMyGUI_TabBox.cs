using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LapinerTools.uMyGUI;

public class uMyGUI_TabBox : MonoBehaviour
{
	public enum EAnimMode
	{
		NONE,
		TAB_ONLY,
		BTN_ONLY,
		TAB_AND_BTN
	}

	public const string SEND_MESSAGE_ACTIVATE_NAME = "uMyGUI_OnActivateTab";

	public const string SEND_MESSAGE_DEACTIVATE_NAME = "uMyGUI_OnDeactivateTab";

	[SerializeField]
	private RectTransform[] m_btns = new RectTransform[0];

	[SerializeField]
	private RectTransform[] m_tabs = new RectTransform[0];

	[SerializeField]
	private int m_selectedIndex;

	[SerializeField]
	private bool m_isSelectTabOnStart = true;

	[SerializeField]
	private bool m_isPlayTabAnimOnStart = true;

	[SerializeField]
	private bool m_isPlayBtnAnimOnStart = true;

	[SerializeField]
	private EAnimMode m_animMode;

	[SerializeField]
	private string m_fadeInAnimTab = "tab_fade_in";

	[SerializeField]
	private string m_fadeOutAnimTab = "tab_fade_out";

	[SerializeField]
	private string m_fadeInAnimBtn = "btn_fade_in";

	[SerializeField]
	private string m_fadeOutAnimBtn = "btn_fade_out";

	[SerializeField]
	private bool m_isSendMessage;

	[SerializeField]
	private bool m_isMoveDownInHierarchyOnSelect;

	public void SelectTab(int p_tabIndex)
	{
		if (p_tabIndex == m_selectedIndex)
		{
			return;
		}
		if (p_tabIndex >= 0 && p_tabIndex < m_tabs.Length)
		{
			if (m_tabs[p_tabIndex] == null)
			{
				Debug.LogError("uMyGUI_TabBox: SelectTab tab index '" + p_tabIndex + "' is null! Check the tabs array in the inspector!");
				return;
			}
			if (m_isMoveDownInHierarchyOnSelect)
			{
				m_tabs[p_tabIndex].SetAsLastSibling();
			}
			switch (m_animMode)
			{
			case EAnimMode.TAB_ONLY:
				StopAllCoroutines();
				AnimateRectRectTransformSelection(p_tabIndex, m_tabs, m_fadeInAnimTab, m_fadeOutAnimTab, p_isActivateChanged: true);
				break;
			case EAnimMode.BTN_ONLY:
				StopAllCoroutines();
				AnimateRectRectTransformSelection(p_tabIndex, m_btns, m_fadeInAnimBtn, m_fadeOutAnimBtn, p_isActivateChanged: false);
				UpdateTabActiveStates(p_tabIndex);
				break;
			case EAnimMode.TAB_AND_BTN:
				StopAllCoroutines();
				AnimateRectRectTransformSelection(p_tabIndex, m_tabs, m_fadeInAnimTab, m_fadeOutAnimTab, p_isActivateChanged: true);
				AnimateRectRectTransformSelection(p_tabIndex, m_btns, m_fadeInAnimBtn, m_fadeOutAnimBtn, p_isActivateChanged: false);
				break;
			default:
				UpdateTabActiveStates(p_tabIndex);
				break;
			}
			m_selectedIndex = p_tabIndex;
		}
		else
		{
			Debug.LogError("uMyGUI_TabBox: SelectTab tab index '" + p_tabIndex + "' is out of bounds [0," + m_tabs.Length + "]!");
		}
	}

	private void Start()
	{
		if (m_isSelectTabOnStart)
		{
			UpdateTabActiveStates(m_selectedIndex);
			if (m_isPlayTabAnimOnStart && (m_animMode == EAnimMode.TAB_ONLY || m_animMode == EAnimMode.TAB_AND_BTN))
			{
				AnimateRectRectTransformSelection(m_selectedIndex, m_tabs, m_fadeInAnimTab, m_fadeOutAnimTab, p_isActivateChanged: false);
			}
			if (m_isPlayBtnAnimOnStart)
			{
				AnimateRectRectTransformSelection(m_selectedIndex, m_btns, m_fadeInAnimBtn, m_fadeOutAnimBtn, p_isActivateChanged: false);
			}
		}
	}

	private void UpdateTabActiveStates(int p_tabIndex)
	{
		for (int i = 0; i < m_tabs.Length; i++)
		{
			bool flag = i == p_tabIndex;
			if (m_tabs[i] != null)
			{
				if (m_isSendMessage)
				{
					m_tabs[i].gameObject.SendMessage(flag ? "uMyGUI_OnActivateTab" : "uMyGUI_OnDeactivateTab", SendMessageOptions.DontRequireReceiver);
				}
				m_tabs[i].gameObject.SetActive(flag);
			}
			if (m_btns.Length > i && m_btns[i] != null)
			{
				Selectable component = m_btns[i].GetComponent<Selectable>();
				if (component != null)
				{
					component.interactable = !flag;
				}
			}
		}
	}

	private void AnimateRectRectTransformSelection(int p_tabIndex, RectTransform[] p_transforms, string p_fadeInAnim, string p_fadeOutAnim, bool p_isActivateChanged)
	{
		for (int i = 0; i < p_transforms.Length; i++)
		{
			if (!(p_transforms[i] != null))
			{
				continue;
			}
			Animation component = p_transforms[i].GetComponent<Animation>();
			if (component != null)
			{
				if (i == m_selectedIndex && p_tabIndex != m_selectedIndex)
				{
					if (component[p_fadeOutAnim] != null)
					{
						if (p_isActivateChanged)
						{
							StartCoroutine(DeactivateAfterDelay(p_transforms[i].gameObject, component[p_fadeOutAnim].length));
						}
						if (m_isSendMessage)
						{
							p_transforms[i].gameObject.SendMessage("uMyGUI_OnDeactivateTab", SendMessageOptions.DontRequireReceiver);
						}
						component.Play(p_fadeOutAnim);
					}
				}
				else if (i == p_tabIndex)
				{
					if (p_isActivateChanged)
					{
						p_transforms[i].gameObject.SetActive(value: true);
					}
					if (m_isSendMessage)
					{
						p_transforms[i].gameObject.SendMessage("uMyGUI_OnActivateTab", SendMessageOptions.DontRequireReceiver);
					}
					component.Play(p_fadeInAnim);
				}
			}
			else
			{
				Debug.LogError("uMyGUI_TabBox: AnimateRectRectTransformSelection: object at index '" + i + "' has no Animation component and cannot fade in or out!");
			}
			Selectable component2 = p_transforms[i].GetComponent<Selectable>();
			if (component2 != null)
			{
				component2.interactable = i != p_tabIndex;
			}
		}
	}

	private IEnumerator DeactivateAfterDelay(GameObject p_object, float p_delay)
	{
		yield return new WaitForSeconds(p_delay);
		if (p_object != null)
		{
			p_object.SetActive(value: false);
		}
	}
}
