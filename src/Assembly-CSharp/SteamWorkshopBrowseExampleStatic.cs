using System.IO;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopBrowseExampleStatic : MonoBehaviour
{
	private void Start()
	{
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;
		if (SWBParentUI.Instance == null)
		{
			string text = "SteamWorkshopBrowseExampleStatic: you have no SteamWorkshopUIBrowse in this scene! Please drag an drop the 'SteamWorkshopItemBrowser' prefab from 'LapinerTools/Steam/Workshop' into your Canvas object!";
			Debug.LogError(text);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Error", text);
			return;
		}
		SWBParentUI.Instance.onPlayButtonClick += delegate(WorkshopItemEventArgs p_itemArgs)
		{
			string text2 = "\n";
			try
			{
				string[] files = Directory.GetFiles(p_itemArgs.Item.InstalledLocalFolder);
				for (int i = 0; i < files.Length; i++)
				{
					text2 = text2 + files[i] + "\n";
				}
			}
			catch
			{
				text2 += "not found!";
			}
			string text3 = "Name: " + p_itemArgs.Item.Name + "\nPublished File Id: " + p_itemArgs.Item.SteamNative.m_nPublishedFileId.ToString() + "\nLocal Folder: " + p_itemArgs.Item.InstalledLocalFolder + "\n" + text2;
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Item Played", "Load your Steam Workshop item here (e.g. could be a new level for your game)\n" + text3).ShowButton("ok");
		};
	}
}
