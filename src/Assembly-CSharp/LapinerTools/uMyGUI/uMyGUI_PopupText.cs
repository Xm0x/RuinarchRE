using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LapinerTools.uMyGUI;

public class uMyGUI_PopupText : uMyGUI_PopupButtons
{
	[SerializeField]
	protected Text m_header;

	[SerializeField]
	protected Text m_body;

	[SerializeField]
	protected bool m_useExplicitNavigation;

	protected bool m_isFirstFrameShown;

	public virtual uMyGUI_PopupText SetText(string p_headerText, string p_bodyText)
	{
		if (m_header != null)
		{
			m_header.text = p_headerText;
		}
		if (m_body != null)
		{
			m_body.text = p_bodyText;
		}
		return this;
	}

	public override void Show()
	{
		base.Show();
		m_isFirstFrameShown = true;
	}

	public virtual void LateUpdate()
	{
		if (!m_isFirstFrameShown)
		{
			return;
		}
		m_isFirstFrameShown = false;
		if (!m_useExplicitNavigation)
		{
			return;
		}
		List<Button> list = new List<Button>();
		for (int i = 0; i < m_buttons.Length; i++)
		{
			if (m_buttons[i] != null && m_buttons[i].gameObject.activeSelf && m_buttons[i].GetComponentInChildren<Button>() != null)
			{
				list.Add(m_buttons[i].GetComponentInChildren<Button>());
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Button button = list[j];
			Navigation navigation = button.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			if (j > 0)
			{
				navigation.selectOnLeft = list[j - 1];
			}
			if (j < list.Count - 1)
			{
				navigation.selectOnRight = list[j + 1];
			}
			button.navigation = navigation;
		}
	}
}
