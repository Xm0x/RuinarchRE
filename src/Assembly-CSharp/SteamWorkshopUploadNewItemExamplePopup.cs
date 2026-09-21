using System;
using System.IO;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopUploadNewItemExamplePopup : MonoBehaviour
{
	private void Start()
	{
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;
		string text = Path.Combine(Application.persistentDataPath, "DummyItemContentFolder" + DateTime.Now.Ticks);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string contents = "Save your item/level/mod data here.\nIt does not need to be a text file. Any file type is supported (binary, images, etc...).\nYou can save multiple files, Steam items are folders (not single files).\n";
		File.WriteAllText(Path.Combine(text, "ItemData.txt"), contents);
		WorkshopItemUpdate workshopItemUpdate = new WorkshopItemUpdate();
		workshopItemUpdate.ContentPath = text;
		((SteamWorkshopPopupUpload)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_upload")).UploadUI.SetItemData(workshopItemUpdate);
	}
}
