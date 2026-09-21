using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class FactionSettingVillageEditorUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickAddVillage();

		void OnClickClose();
	}

	public FactionSettingVillageEditorUIModel UIModel => _baseAssetModel as FactionSettingVillageEditorUIModel;

	public static void Create(Canvas p_canvas, FactionSettingVillageEditorUIModel p_assets, Action<FactionSettingVillageEditorUIView> p_onCreate)
	{
		FactionSettingVillageEditorUIView factionSettingVillageEditorUIView = new GameObject(typeof(FactionSettingVillageEditorUIView).ToString()).AddComponent<FactionSettingVillageEditorUIView>();
		FactionSettingVillageEditorUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		factionSettingVillageEditorUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(factionSettingVillageEditorUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		FactionSettingVillageEditorUIModel uIModel = UIModel;
		uIModel.onClickAddVillage = (Action)Delegate.Combine(uIModel.onClickAddVillage, new Action(p_listener.OnClickAddVillage));
		FactionSettingVillageEditorUIModel uIModel2 = UIModel;
		uIModel2.onClickClose = (Action)Delegate.Combine(uIModel2.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void Unsubscribe(IListener p_listener)
	{
		FactionSettingVillageEditorUIModel uIModel = UIModel;
		uIModel.onClickAddVillage = (Action)Delegate.Remove(uIModel.onClickAddVillage, new Action(p_listener.OnClickAddVillage));
		FactionSettingVillageEditorUIModel uIModel2 = UIModel;
		uIModel2.onClickClose = (Action)Delegate.Remove(uIModel2.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void InitializeVillageItems()
	{
		List<string> list = RuinarchListPool<string>.Claim();
		Utilities.PopulateLocalizedEnumChoices<VILLAGE_SIZE>(list);
		for (int i = 0; i < UIModel.villageSettingUIItems.Length; i++)
		{
			VillageSettingUIItem obj = UIModel.villageSettingUIItems[i];
			obj.SetVillageSizeChoices(list);
			obj.SetMinusBtnState(i != 0);
		}
		RuinarchListPool<string>.Release(list);
	}

	public void UpdateItemChoices()
	{
		List<string> list = RuinarchListPool<string>.Claim();
		Utilities.PopulateLocalizedEnumChoices<VILLAGE_SIZE>(list);
		for (int i = 0; i < UIModel.villageSettingUIItems.Length; i++)
		{
			UIModel.villageSettingUIItems[i].SetVillageSizeChoices(list);
		}
		RuinarchListPool<string>.Release(list);
	}

	public void UpdateVillageItems(List<VillageSetting> p_villageSettings)
	{
		for (int i = 0; i < UIModel.villageSettingUIItems.Length; i++)
		{
			VillageSettingUIItem villageSettingUIItem = UIModel.villageSettingUIItems[i];
			villageSettingUIItem.SetMinusBtnState(i != 0);
			if (p_villageSettings.IsIndexInList(i))
			{
				VillageSetting itemDetails = p_villageSettings[i];
				villageSettingUIItem.SetItemDetails(itemDetails);
				villageSettingUIItem.gameObject.SetActive(value: true);
			}
			else
			{
				villageSettingUIItem.gameObject.SetActive(value: false);
			}
		}
	}

	public void SetAddVillageBtnState(bool p_state)
	{
		UIModel.btnAddVillage.gameObject.SetActive(p_state);
	}
}
