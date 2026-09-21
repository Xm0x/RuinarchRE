using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UtilityScripts;

public class SpawnPartyUIView : MVCUIView
{
	public interface IListener
	{
		void OnTargetDropdownValueChanged(int p_value);

		void OnHoverOverTargetInDropdown(Transform p_dropDownItem);

		void OnHoverOutTargetInDropdown(Transform p_dropDownItem);

		void OnClickAddPartySlot();

		void OnHoverOverAddPartySlot();

		void OnHoverOutAddPartySlot();

		void OnClickSpawnParty();

		void OnClickClose();

		void OnTargetLocationDropdownValueChanged(int p_value);

		void OnClickCloseObjectPicker();

		void OnBehaviourDropdownValueChanged(int p_value);

		void OnHoverOverBehaviourDropdown(int p_value);

		void OnHoverOutBehaviourDropdown(int p_value);

		void OnHoverOverBehaviourDropdownItem(Transform p_dropDownItem);

		void OnHoverOutBehaviourDropdownItem(Transform p_dropDownItem);
	}

	public List<SpawnPartySlotItem> unlockedSummonSlots = new List<SpawnPartySlotItem>(4);

	public SpawnPartyUIModel UIModel => _baseAssetModel as SpawnPartyUIModel;

	public static void Create(Canvas p_canvas, SpawnPartyUIModel p_assets, Action<SpawnPartyUIView> p_onCreate)
	{
		SpawnPartyUIView spawnPartyUIView = new GameObject(typeof(SpawnPartyUIView).ToString()).AddComponent<SpawnPartyUIView>();
		SpawnPartyUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		spawnPartyUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(spawnPartyUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		SpawnPartyUIModel uIModel = UIModel;
		uIModel.onChooseTarget = (Action<int>)Delegate.Combine(uIModel.onChooseTarget, new Action<int>(p_listener.OnTargetDropdownValueChanged));
		SpawnPartyUIModel uIModel2 = UIModel;
		uIModel2.onHoverOverTargetInDropdown = (Action<Transform>)Delegate.Combine(uIModel2.onHoverOverTargetInDropdown, new Action<Transform>(p_listener.OnHoverOverTargetInDropdown));
		SpawnPartyUIModel uIModel3 = UIModel;
		uIModel3.onHoverOutTargetInDropdown = (Action<Transform>)Delegate.Combine(uIModel3.onHoverOutTargetInDropdown, new Action<Transform>(p_listener.OnHoverOutTargetInDropdown));
		SpawnPartyUIModel uIModel4 = UIModel;
		uIModel4.onClickAddPartySlot = (Action)Delegate.Combine(uIModel4.onClickAddPartySlot, new Action(p_listener.OnClickAddPartySlot));
		SpawnPartyUIModel uIModel5 = UIModel;
		uIModel5.onClickSpawnParty = (Action)Delegate.Combine(uIModel5.onClickSpawnParty, new Action(p_listener.OnClickSpawnParty));
		SpawnPartyUIModel uIModel6 = UIModel;
		uIModel6.onHoverOverAddPartySlot = (Action)Delegate.Combine(uIModel6.onHoverOverAddPartySlot, new Action(p_listener.OnHoverOverAddPartySlot));
		SpawnPartyUIModel uIModel7 = UIModel;
		uIModel7.onHoverOutAddPartySlot = (Action)Delegate.Combine(uIModel7.onHoverOutAddPartySlot, new Action(p_listener.OnHoverOutAddPartySlot));
		SpawnPartyUIModel uIModel8 = UIModel;
		uIModel8.onClickClose = (Action)Delegate.Combine(uIModel8.onClickClose, new Action(p_listener.OnClickClose));
		SpawnPartyUIModel uIModel9 = UIModel;
		uIModel9.onChooseTargetLocation = (Action<int>)Delegate.Combine(uIModel9.onChooseTargetLocation, new Action<int>(p_listener.OnTargetLocationDropdownValueChanged));
		SpawnPartyUIModel uIModel10 = UIModel;
		uIModel10.onClickCloseObjectPicker = (Action)Delegate.Combine(uIModel10.onClickCloseObjectPicker, new Action(p_listener.OnClickCloseObjectPicker));
		SpawnPartyUIModel uIModel11 = UIModel;
		uIModel11.onChooseBehaviour = (Action<int>)Delegate.Combine(uIModel11.onChooseBehaviour, new Action<int>(p_listener.OnBehaviourDropdownValueChanged));
		SpawnPartyUIModel uIModel12 = UIModel;
		uIModel12.onHoverOverBehaviourDropdown = (Action<int>)Delegate.Combine(uIModel12.onHoverOverBehaviourDropdown, new Action<int>(p_listener.OnHoverOverBehaviourDropdown));
		SpawnPartyUIModel uIModel13 = UIModel;
		uIModel13.onHoverOutBehaviourDropdown = (Action<int>)Delegate.Combine(uIModel13.onHoverOutBehaviourDropdown, new Action<int>(p_listener.OnHoverOutBehaviourDropdown));
		SpawnPartyUIModel uIModel14 = UIModel;
		uIModel14.onHoverOverBehaviourDropdownItem = (Action<Transform>)Delegate.Combine(uIModel14.onHoverOverBehaviourDropdownItem, new Action<Transform>(p_listener.OnHoverOverBehaviourDropdownItem));
		SpawnPartyUIModel uIModel15 = UIModel;
		uIModel15.onHoverOutBehaviourDropdownItem = (Action<Transform>)Delegate.Combine(uIModel15.onHoverOutBehaviourDropdownItem, new Action<Transform>(p_listener.OnHoverOutBehaviourDropdownItem));
	}

	public void Unsubscribe(IListener p_listener)
	{
		SpawnPartyUIModel uIModel = UIModel;
		uIModel.onChooseTarget = (Action<int>)Delegate.Remove(uIModel.onChooseTarget, new Action<int>(p_listener.OnTargetDropdownValueChanged));
		SpawnPartyUIModel uIModel2 = UIModel;
		uIModel2.onHoverOverTargetInDropdown = (Action<Transform>)Delegate.Remove(uIModel2.onHoverOverTargetInDropdown, new Action<Transform>(p_listener.OnHoverOverTargetInDropdown));
		SpawnPartyUIModel uIModel3 = UIModel;
		uIModel3.onHoverOutTargetInDropdown = (Action<Transform>)Delegate.Remove(uIModel3.onHoverOutTargetInDropdown, new Action<Transform>(p_listener.OnHoverOutTargetInDropdown));
		SpawnPartyUIModel uIModel4 = UIModel;
		uIModel4.onClickAddPartySlot = (Action)Delegate.Remove(uIModel4.onClickAddPartySlot, new Action(p_listener.OnClickAddPartySlot));
		SpawnPartyUIModel uIModel5 = UIModel;
		uIModel5.onClickSpawnParty = (Action)Delegate.Remove(uIModel5.onClickSpawnParty, new Action(p_listener.OnClickSpawnParty));
		SpawnPartyUIModel uIModel6 = UIModel;
		uIModel6.onHoverOverAddPartySlot = (Action)Delegate.Remove(uIModel6.onHoverOverAddPartySlot, new Action(p_listener.OnHoverOverAddPartySlot));
		SpawnPartyUIModel uIModel7 = UIModel;
		uIModel7.onHoverOutAddPartySlot = (Action)Delegate.Remove(uIModel7.onHoverOutAddPartySlot, new Action(p_listener.OnHoverOutAddPartySlot));
		SpawnPartyUIModel uIModel8 = UIModel;
		uIModel8.onClickClose = (Action)Delegate.Remove(uIModel8.onClickClose, new Action(p_listener.OnClickClose));
		SpawnPartyUIModel uIModel9 = UIModel;
		uIModel9.onChooseTargetLocation = (Action<int>)Delegate.Remove(uIModel9.onChooseTargetLocation, new Action<int>(p_listener.OnTargetLocationDropdownValueChanged));
		SpawnPartyUIModel uIModel10 = UIModel;
		uIModel10.onClickCloseObjectPicker = (Action)Delegate.Remove(uIModel10.onClickCloseObjectPicker, new Action(p_listener.OnClickCloseObjectPicker));
		SpawnPartyUIModel uIModel11 = UIModel;
		uIModel11.onChooseBehaviour = (Action<int>)Delegate.Remove(uIModel11.onChooseBehaviour, new Action<int>(p_listener.OnBehaviourDropdownValueChanged));
		SpawnPartyUIModel uIModel12 = UIModel;
		uIModel12.onHoverOverBehaviourDropdown = (Action<int>)Delegate.Remove(uIModel12.onHoverOverBehaviourDropdown, new Action<int>(p_listener.OnHoverOverBehaviourDropdown));
		SpawnPartyUIModel uIModel13 = UIModel;
		uIModel13.onHoverOutBehaviourDropdown = (Action<int>)Delegate.Remove(uIModel13.onHoverOutBehaviourDropdown, new Action<int>(p_listener.OnHoverOutBehaviourDropdown));
		SpawnPartyUIModel uIModel14 = UIModel;
		uIModel14.onHoverOverBehaviourDropdownItem = (Action<Transform>)Delegate.Remove(uIModel14.onHoverOverBehaviourDropdownItem, new Action<Transform>(p_listener.OnHoverOverBehaviourDropdownItem));
		SpawnPartyUIModel uIModel15 = UIModel;
		uIModel15.onHoverOutBehaviourDropdownItem = (Action<Transform>)Delegate.Remove(uIModel15.onHoverOutBehaviourDropdownItem, new Action<Transform>(p_listener.OnHoverOutBehaviourDropdownItem));
	}

	public void SetTitle(string p_title)
	{
		UIModel.lblTitle.text = p_title;
	}

	public void SetSelectTargetTitle(string p_title)
	{
		UIModel.lblSelectTargetTitle.text = p_title;
	}

	public void UpdateTargetOptions(List<IStoredTarget> p_targets)
	{
		UIModel.dropDownTargets.ClearOptions();
		List<TMP_Dropdown.OptionData> list = RuinarchListPool<TMP_Dropdown.OptionData>.Claim();
		for (int i = 0; i < p_targets.Count; i++)
		{
			IStoredTarget storedTarget = p_targets[i];
			TMP_Dropdown.OptionData item = new TMP_Dropdown.OptionData(storedTarget.bookmarkName, storedTarget.GetPortraitSprite());
			list.Add(item);
		}
		UIModel.dropDownTargets.AddOptions(list);
		RuinarchListPool<TMP_Dropdown.OptionData>.Release(list);
	}

	public void UpdateTargetDisplay(IStoredTarget p_target, int p_targetIndex)
	{
		UIModel.dropDownTargets.SetValueWithoutNotify(p_targetIndex);
		UIModel.lblTargetName.text = p_target.bookmarkName;
	}

	public void SetTargetDropdownInteractableState(bool p_interactable)
	{
		UIModel.dropDownTargets.interactable = p_interactable;
	}

	public void UpdateUnlockedSummonSlots(int p_unlockedSlots)
	{
		unlockedSummonSlots.Clear();
		for (int i = 0; i < UIModel.summonSlotItems.Length; i++)
		{
			SpawnPartySlotItem spawnPartySlotItem = UIModel.summonSlotItems[i];
			bool flag = i < p_unlockedSlots;
			spawnPartySlotItem.gameObject.SetActive(flag);
			if (flag)
			{
				unlockedSummonSlots.Add(spawnPartySlotItem);
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

	public void SetStructureResidentsState(bool p_state)
	{
		UIModel.goStructureResidents.SetActive(p_state);
	}

	public void SetDemonLeaderAndSummonsState(bool p_state)
	{
		UIModel.goDemonicSummonsAndLeader.SetActive(p_state);
	}

	public void UpdateStructureResidents(List<Character> p_residents)
	{
		Utilities.DestroyChildrenObjectPool(UIModel.structureResidentsParent);
		for (int i = 0; i < p_residents.Count; i++)
		{
			Character character = p_residents[i];
			CharacterPortrait component = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterPortrait", Vector3.zero, Quaternion.identity, UIModel.structureResidentsParent).GetComponent<CharacterPortrait>();
			component.GeneratePortrait(character);
			component.SetFactionEmblemState(p_state: false);
			component.SetLeaderIconState(p_state: false);
		}
	}

	public void SetSpawnPartyInteractableState(bool p_state)
	{
		UIModel.btnSpawnParty.interactable = p_state;
	}

	public void SetSpawnPartyBtnLabelName(string p_title)
	{
		UIModel.lblSpawnParty.text = p_title;
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

	public void ShowCharacterTooltip(Character p_data)
	{
		UIModel.characterNameplateTooltip.SetObject(p_data);
		UIModel.characterNameplateTooltip.gameObject.SetActive(value: true);
	}

	public void HideCharacterTooltip()
	{
		UIModel.characterNameplateTooltip.gameObject.SetActive(value: false);
	}

	public void SetTargetLocationState(bool p_state)
	{
		UIModel.goTargetLocation.SetActive(p_state);
	}

	public void SetTargetLocationDropdownOptions(List<TMP_Dropdown.OptionData> p_options)
	{
		UIModel.dropDownTargetLocations.ClearOptions();
		UIModel.dropDownTargetLocations.AddOptions(p_options);
	}

	public void UpdateAttackerLocationDisplay(LocationStructure p_target, int p_targetIndex)
	{
		UIModel.dropDownTargetLocations.SetValueWithoutNotify(p_targetIndex);
		UIModel.lblTargetLocation.text = p_target.bookmarkName;
	}

	public void SetAttackerDropdownInteractableState(bool p_interactable)
	{
		UIModel.dropDownTargetLocations.interactable = p_interactable;
	}

	public bool IsObjectPickerShowing()
	{
		return UIModel.goObjectPicker.activeInHierarchy;
	}

	public void ShowObjectPicker(List<Character> p_choices, Action<Character> p_onClick, Action<Character> p_onHoverOver, Action<Character> p_onHoverOut, Func<Character, bool> p_canChooseCharacter)
	{
		Utilities.DestroyChildrenObjectPool(UIModel.scrollRectObjectPicker.content);
		for (int i = 0; i < p_choices.Count; i++)
		{
			Character character = p_choices[i];
			SpawnPartyObjectPickerItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(UIModel.prefabObjectPicker.name, Vector3.zero, Quaternion.identity, UIModel.scrollRectObjectPicker.content).GetComponent<SpawnPartyObjectPickerItem>();
			component.Initialize(character, p_onClick, p_onHoverOver, p_onHoverOut);
			component.SetInteractableState(p_canChooseCharacter(character));
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

	public void UpdateBehaviourOptions(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR[] p_behaviours)
	{
		UIModel.dropDownBehaviours.ClearOptions();
		List<TMP_Dropdown.OptionData> list = RuinarchListPool<TMP_Dropdown.OptionData>.Claim();
		foreach (SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_type in p_behaviours)
		{
			TMP_Dropdown.OptionData item = new TMP_Dropdown.OptionData(LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", p_type.ToStringEnumWithSpace()), null);
			list.Add(item);
		}
		UIModel.dropDownBehaviours.AddOptions(list);
		RuinarchListPool<TMP_Dropdown.OptionData>.Release(list);
	}

	public void UpdateBehaviourDropdownDisplay(int p_targetIndex)
	{
		UIModel.dropDownBehaviours.SetValueWithoutNotify(p_targetIndex);
	}
}
