using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UtilityScripts;

public class SnatchObjectUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickAddPartySlot();

		void OnHoverOverAddPartySlot();

		void OnHoverOutAddPartySlot();

		void OnClickSnatchObject();

		void OnClickClose();

		void OnTargetLocationDropdownValueChanged(int p_value);

		void OnClickCloseObjectPicker();
	}

	public List<SnatchObjectSlotItem> unlockedSummonSlots = new List<SnatchObjectSlotItem>(4);

	public SnatchObjectUIModel UIModel => _baseAssetModel as SnatchObjectUIModel;

	public static void Create(Canvas p_canvas, SnatchObjectUIModel p_assets, Action<SnatchObjectUIView> p_onCreate)
	{
		SnatchObjectUIView snatchObjectUIView = new GameObject(typeof(SnatchObjectUIView).ToString()).AddComponent<SnatchObjectUIView>();
		SnatchObjectUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		snatchObjectUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(snatchObjectUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		SnatchObjectUIModel uIModel = UIModel;
		uIModel.onClickAddPartySlot = (Action)Delegate.Combine(uIModel.onClickAddPartySlot, new Action(p_listener.OnClickAddPartySlot));
		SnatchObjectUIModel uIModel2 = UIModel;
		uIModel2.onClickSnatchObject = (Action)Delegate.Combine(uIModel2.onClickSnatchObject, new Action(p_listener.OnClickSnatchObject));
		SnatchObjectUIModel uIModel3 = UIModel;
		uIModel3.onHoverOverAddPartySlot = (Action)Delegate.Combine(uIModel3.onHoverOverAddPartySlot, new Action(p_listener.OnHoverOverAddPartySlot));
		SnatchObjectUIModel uIModel4 = UIModel;
		uIModel4.onHoverOutAddPartySlot = (Action)Delegate.Combine(uIModel4.onHoverOutAddPartySlot, new Action(p_listener.OnHoverOutAddPartySlot));
		SnatchObjectUIModel uIModel5 = UIModel;
		uIModel5.onClickClose = (Action)Delegate.Combine(uIModel5.onClickClose, new Action(p_listener.OnClickClose));
		SnatchObjectUIModel uIModel6 = UIModel;
		uIModel6.onChooseTargetLocation = (Action<int>)Delegate.Combine(uIModel6.onChooseTargetLocation, new Action<int>(p_listener.OnTargetLocationDropdownValueChanged));
		SnatchObjectUIModel uIModel7 = UIModel;
		uIModel7.onClickCloseObjectPicker = (Action)Delegate.Combine(uIModel7.onClickCloseObjectPicker, new Action(p_listener.OnClickCloseObjectPicker));
	}

	public void Unsubscribe(IListener p_listener)
	{
		SnatchObjectUIModel uIModel = UIModel;
		uIModel.onClickAddPartySlot = (Action)Delegate.Remove(uIModel.onClickAddPartySlot, new Action(p_listener.OnClickAddPartySlot));
		SnatchObjectUIModel uIModel2 = UIModel;
		uIModel2.onClickSnatchObject = (Action)Delegate.Remove(uIModel2.onClickSnatchObject, new Action(p_listener.OnClickSnatchObject));
		SnatchObjectUIModel uIModel3 = UIModel;
		uIModel3.onHoverOverAddPartySlot = (Action)Delegate.Remove(uIModel3.onHoverOverAddPartySlot, new Action(p_listener.OnHoverOverAddPartySlot));
		SnatchObjectUIModel uIModel4 = UIModel;
		uIModel4.onHoverOutAddPartySlot = (Action)Delegate.Remove(uIModel4.onHoverOutAddPartySlot, new Action(p_listener.OnHoverOutAddPartySlot));
		SnatchObjectUIModel uIModel5 = UIModel;
		uIModel5.onClickClose = (Action)Delegate.Remove(uIModel5.onClickClose, new Action(p_listener.OnClickClose));
		SnatchObjectUIModel uIModel6 = UIModel;
		uIModel6.onChooseTargetLocation = (Action<int>)Delegate.Remove(uIModel6.onChooseTargetLocation, new Action<int>(p_listener.OnTargetLocationDropdownValueChanged));
		SnatchObjectUIModel uIModel7 = UIModel;
		uIModel7.onClickCloseObjectPicker = (Action)Delegate.Remove(uIModel7.onClickCloseObjectPicker, new Action(p_listener.OnClickCloseObjectPicker));
	}

	public void SetTitle(string p_title)
	{
		UIModel.lblTitle.text = p_title;
	}

	public void UpdateTargetDisplay(IStoredTarget p_target)
	{
		UIModel.lblTargetName.text = p_target.bookmarkName;
	}

	public void UpdateUnlockedSummonSlots(int p_unlockedSlots)
	{
		unlockedSummonSlots.Clear();
		for (int i = 0; i < UIModel.summonSlotItems.Length; i++)
		{
			SnatchObjectSlotItem snatchObjectSlotItem = UIModel.summonSlotItems[i];
			bool flag = i < p_unlockedSlots;
			snatchObjectSlotItem.gameObject.SetActive(flag);
			if (flag)
			{
				unlockedSummonSlots.Add(snatchObjectSlotItem);
			}
		}
	}

	public void SetUnlockSlotBtnState(bool p_state)
	{
		UIModel.btnAddPartySlot.gameObject.SetActive(p_state);
	}

	public void SetUnlockSlotBtnInteractable(bool p_interactable)
	{
		UIModel.btnAddPartySlot.interactable = p_interactable;
	}

	public void SetNoSummonsGOState(bool p_state)
	{
		UIModel.goNoSummons.SetActive(p_state);
	}

	public void SetSnatchObjectInteractableState(bool p_state)
	{
		UIModel.btnSnatchObject.interactable = p_state;
	}

	public void SetSnatchObjectBtnLabelName(string p_title)
	{
		UIModel.lblSnatchObject.text = p_title;
	}

	public void ShowMonsterTooltip(MonsterAndDemonUnderlingCharges p_data)
	{
		UIModel.underlingTooltip.SetObject(p_data);
		UIModel.underlingTooltip.gameObject.SetActive(value: true);
	}

	public void HideMonsterTooltip()
	{
		UIModel.underlingTooltip.gameObject.SetActive(value: false);
	}

	public void SetTargetLocationState(bool p_state)
	{
		UIModel.goTargetLocation.SetActive(p_state);
	}

	public void SetTargetLocationInteractableState(bool p_state)
	{
		UIModel.dropDownTargetLocations.interactable = p_state;
	}

	public void SetTargetLocationDropdownOptions(List<TMP_Dropdown.OptionData> p_options)
	{
		UIModel.dropDownTargetLocations.ClearOptions();
		UIModel.dropDownTargetLocations.AddOptions(p_options);
	}

	public void UpdateTargetLocationDisplay(LocationStructure p_target, int p_targetIndex)
	{
		UIModel.dropDownTargetLocations.SetValueWithoutNotify(p_targetIndex);
		UIModel.lblTargetLocation.text = p_target.bookmarkName;
	}

	public bool IsObjectPickerShowing()
	{
		return UIModel.goObjectPicker.activeInHierarchy;
	}

	public void ShowObjectPicker(List<Character> p_choices, Action<Character> p_onClick, Action<Character> p_onHoverOver, Action<Character> p_onHoverOut, Func<Character, bool> p_canChooseMonsterUnderling)
	{
		Utilities.DestroyChildrenObjectPool(UIModel.scrollRectObjectPicker.content);
		for (int i = 0; i < p_choices.Count; i++)
		{
			Character character = p_choices[i];
			SpawnPartyObjectPickerItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(UIModel.prefabObjectPicker.name, Vector3.zero, Quaternion.identity, UIModel.scrollRectObjectPicker.content).GetComponent<SpawnPartyObjectPickerItem>();
			component.Initialize(character, p_onClick, p_onHoverOver, p_onHoverOut);
			component.SetInteractableState(p_canChooseMonsterUnderling(character));
		}
		UIModel.goObjectPicker.SetActive(value: true);
	}

	public void UpdateObjectPickerItems(Func<Character, bool> p_canChooseMonsterUnderling)
	{
		SpawnPartyObjectPickerItem[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<SpawnPartyObjectPickerItem>(UIModel.scrollRectObjectPicker.content.gameObject);
		foreach (SpawnPartyObjectPickerItem spawnPartyObjectPickerItem in componentsInDirectChildren)
		{
			spawnPartyObjectPickerItem.SetInteractableState(p_canChooseMonsterUnderling(spawnPartyObjectPickerItem.character));
		}
	}

	public void HideObjectPicker()
	{
		UIModel.goObjectPicker.SetActive(value: false);
	}

	public void ShowCharacterTooltip(Character p_data)
	{
		UIModel.characterNameplateTooltip.SetObject(p_data);
		UIModel.characterNameplateTooltip.gameObject.SetActive(value: true);
	}

	public void HideCharacterTooltip()
	{
		UIModel.characterNameplateTooltip.gameObject.SetActive(value: false);
	}
}
