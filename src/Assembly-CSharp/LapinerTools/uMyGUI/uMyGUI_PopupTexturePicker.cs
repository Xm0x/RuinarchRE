using System;
using UnityEngine;

namespace LapinerTools.uMyGUI;

public class uMyGUI_PopupTexturePicker : uMyGUI_PopupText
{
	[SerializeField]
	protected uMyGUI_TexturePicker m_picker;

	public override void Hide()
	{
		base.Hide();
		if (m_picker != null)
		{
			m_picker.ButtonCallback = null;
		}
	}

	public virtual uMyGUI_PopupText SetPicker(Texture2D[] p_textures, int p_selectedIndex, Action<int> p_buttonCallback)
	{
		if (m_picker != null)
		{
			m_picker.ButtonCallback = delegate(int p_clickedIndex)
			{
				p_buttonCallback(p_clickedIndex);
				Hide();
			};
			m_picker.SetTextures(p_textures, p_selectedIndex);
		}
		return this;
	}
}
