using System;
using System.Collections.Generic;
using System.IO;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopUpdateItemFromFolderExamplePopup : MonoBehaviour
{
	private void Start()
	{
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;
		List<WorkshopItemUpdate> itemsAvailableForUpdate = new List<WorkshopItemUpdate>();
		string[] directories = Directory.GetDirectories(Application.persistentDataPath);
		foreach (string p_itemContentFolderPath in directories)
		{
			WorkshopItemUpdate itemUpdateFromFolder = SteamWorkshopMain.Instance.GetItemUpdateFromFolder(p_itemContentFolderPath);
			if (itemUpdateFromFolder != null)
			{
				itemsAvailableForUpdate.Add(itemUpdateFromFolder);
			}
		}
		if (itemsAvailableForUpdate.Count > 0)
		{
			string[] array = new string[itemsAvailableForUpdate.Count];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = itemsAvailableForUpdate[j].Name;
			}
			((uMyGUI_PopupDropdown)uMyGUI_PopupManager.Instance.ShowPopup("dropdown")).SetEntries(array).SetOnSelected(delegate(int p_selectedIndex)
			{
				OnExistingItemSelectedForUpdate(itemsAvailableForUpdate[p_selectedIndex]);
			}).SetText("Select Item", "Select the item, which you want to update");
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("No Items Found", "Cannot find any items to update, it seems that you haven't uploaded any items yet.").ShowButton("ok");
		}
	}

	private void OnExistingItemSelectedForUpdate(WorkshopItemUpdate p_updateExistingItem)
	{
		uMyGUI_PopupManager.Instance.HidePopup("dropdown");
		string path = Path.Combine(p_updateExistingItem.ContentPath, "ItemData.txt");
		if (File.Exists(path))
		{
			File.AppendAllText(path, "\nUpdate - " + DateTime.Now.ToString());
			((SteamWorkshopPopupUpload)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_upload")).UploadUI.SetItemData(p_updateExistingItem);
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Content File Is Missing", "Have you changed this item's data?!").ShowButton("ok");
		}
	}
}
