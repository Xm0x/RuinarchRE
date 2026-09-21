using System;
using System.IO;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopUpdateOwnedItemExampleStatic : MonoBehaviour
{
	private void Start()
	{
		SteamWorkshopMain.Instance.IsDebugLogEnabled = true;
		if (SteamWorkshopUIUpload.Instance == null)
		{
			string text = "SteamWorkshopUpdateOwnedItemExampleStatic: you have no SteamWorkshopUIUpload in this scene! Please drag an drop the 'SteamWorkshopItemUpload' prefab from 'LapinerTools/Steam/Workshop' into your Canvas object!";
			Debug.LogError(text);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Error", text);
		}
		else
		{
			uMyGUI_PopupManager.Instance.ShowPopup("loading");
			SteamWorkshopMain.Instance.Sorting = new WorkshopSortMode(EWorkshopSource.OWNED);
			SteamWorkshopMain.Instance.GetItemList(1u, OnOwnedItemListLoaded);
		}
	}

	private void OnOwnedItemListLoaded(WorkshopItemListEventArgs p_itemListArgs)
	{
		uMyGUI_PopupManager.Instance.HidePopup("loading");
		if (p_itemListArgs.IsError)
		{
			return;
		}
		if (p_itemListArgs.ItemList.Items.Count > 0)
		{
			string[] array = new string[p_itemListArgs.ItemList.Items.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = p_itemListArgs.ItemList.Items[i].Name;
			}
			((uMyGUI_PopupDropdown)uMyGUI_PopupManager.Instance.ShowPopup("dropdown")).SetEntries(array).SetOnSelected(delegate(int p_selectedIndex)
			{
				OnOwnedItemSelectedForUpdate(p_itemListArgs.ItemList.Items[p_selectedIndex]);
			}).SetText("Select Item", "Select the item, which you want to update");
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("No Items Found", "Cannot find any items to update, it seems that you haven't uploaded any items yet.").ShowButton("ok");
		}
	}

	private void OnOwnedItemSelectedForUpdate(WorkshopItem p_item)
	{
		uMyGUI_PopupManager.Instance.HidePopup("dropdown");
		if (!p_item.IsInstalled)
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Not Installed", "This item is not installed. Please subscribe this item first!").ShowButton("ok");
			Start();
			return;
		}
		WorkshopItemUpdate workshopItemUpdate = new WorkshopItemUpdate(p_item);
		string path = Path.Combine(workshopItemUpdate.ContentPath, "ItemData.txt");
		if (File.Exists(path))
		{
			File.AppendAllText(path, "\nUpdate - " + DateTime.Now.ToString());
			SteamWorkshopUIUpload.Instance.SetItemData(workshopItemUpdate);
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Not Installed", "This item is subscribed, but not installed. Please sync local files in Steam!").ShowButton("ok");
			Start();
		}
	}
}
