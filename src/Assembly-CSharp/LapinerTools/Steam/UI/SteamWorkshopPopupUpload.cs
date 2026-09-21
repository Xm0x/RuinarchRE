using LapinerTools.uMyGUI;
using UnityEngine;

namespace LapinerTools.Steam.UI;

public class SteamWorkshopPopupUpload : uMyGUI_Popup
{
	[SerializeField]
	protected SteamWorkshopUIUpload m_uploadUI;

	public SteamWorkshopUIUpload UploadUI => m_uploadUI;

	public SteamWorkshopPopupUpload()
	{
		DestroyOnHide = true;
	}

	protected override void Start()
	{
		base.Start();
		if (m_uploadUI != null)
		{
			m_uploadUI.OnFinishedUpload += delegate
			{
				Hide();
			};
		}
	}
}
