using System;
using System.IO;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopUploadNewItemExampleStatic : MonoBehaviour
{
	private void Start()
	{
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;
		if (SteamWorkshopUIUpload.Instance == null)
		{
			string text = "SteamWorkshopUploadNewItemExampleStatic: you have no SteamWorkshopUIUpload in this scene! Please drag an drop the 'SteamWorkshopItemUpload' prefab from 'LapinerTools/Steam/Workshop' into your Canvas object!";
			Debug.LogError(text);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Error", text);
			return;
		}
		string text2 = Path.Combine(Application.persistentDataPath, "DummyItemContentFolder" + DateTime.Now.Ticks);
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		string contents = "Save your item/level/mod data here.\nIt does not need to be a text file. Any file type is supported (binary, images, etc...).\nYou can save multiple files, Steam items are folders (not single files).\n";
		File.WriteAllText(Path.Combine(text2, "ItemData.txt"), contents);
		WorkshopItemUpdate workshopItemUpdate = new WorkshopItemUpdate();
		workshopItemUpdate.ContentPath = text2;
		SteamWorkshopUIUpload.Instance.SetItemData(workshopItemUpdate);
	}
}
