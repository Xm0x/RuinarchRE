using System;
using UnityEngine;

namespace LapinerTools.uMyGUI;

public class uMyGUI_PopupDropdown : uMyGUI_PopupText
{
	[SerializeField]
	protected uMyGUI_Dropdown m_dropdown;

	protected Action<int> m_onSelected;

	public override void Show()
	{
		base.Show();
		if (m_dropdown != null)
		{
			m_dropdown.Select(-1);
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (m_dropdown != null && m_onSelected != null)
		{
			m_dropdown.OnSelected -= m_onSelected;
			m_onSelected = null;
		}
	}

	public virtual uMyGUI_PopupDropdown SetEntries(string[] p_entries)
	{
		if (m_dropdown != null)
		{
			m_dropdown.Entries = p_entries;
		}
		return this;
	}

	public virtual uMyGUI_PopupDropdown SetOnSelected(Action<int> p_onSelected)
	{
		if (m_dropdown != null)
		{
			m_onSelected = p_onSelected;
			m_dropdown.OnSelected += p_onSelected;
		}
		return this;
	}
}
