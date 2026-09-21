using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.Localization;

public class FactionSettingVillageEditorUIController : MVCUIController, FactionSettingVillageEditorUIView.IListener
{
	[SerializeField]
	private FactionSettingVillageEditorUIModel m_factionSettingVillageEditorUIModel;

	private FactionSettingVillageEditorUIView m_factionSettingVillageEditorUIView;

	private FactionTemplate _currentlyEditingFaction;

	private Action onHideAction;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		FactionSettingVillageEditorUIView.Create(_canvas, m_factionSettingVillageEditorUIModel, delegate(FactionSettingVillageEditorUIView p_ui)
		{
			m_factionSettingVillageEditorUIView = p_ui;
			m_factionSettingVillageEditorUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			m_factionSettingVillageEditorUIView.InitializeVillageItems();
		});
	}

	public void OnLocaleChanged(Locale p_newLang)
	{
		m_factionSettingVillageEditorUIView.UpdateItemChoices();
	}

	public void SetOnHideAction(Action p_hideAction)
	{
		onHideAction = (Action)Delegate.Combine(onHideAction, p_hideAction);
	}

	public void EditVillageSettings(FactionTemplate p_FactionTemplate)
	{
		_currentlyEditingFaction = p_FactionTemplate;
		ShowUI();
		UpdateVillageItems();
	}

	private void OnEnable()
	{
		VillageSettingUIItem.onClickMinus = (Action<VillageSettingUIItem>)Delegate.Combine(VillageSettingUIItem.onClickMinus, new Action<VillageSettingUIItem>(OnClickMinus));
		VillageSettingUIItem.onChangeName = (Action<VillageSettingUIItem, string>)Delegate.Combine(VillageSettingUIItem.onChangeName, new Action<VillageSettingUIItem, string>(OnChangeFactionName));
		VillageSettingUIItem.onClickRandomizeName = (Action<VillageSettingUIItem>)Delegate.Combine(VillageSettingUIItem.onClickRandomizeName, new Action<VillageSettingUIItem>(OnClickRandomizeName));
		VillageSettingUIItem.onChangeVillageSize = (Action<VillageSettingUIItem, VILLAGE_SIZE>)Delegate.Combine(VillageSettingUIItem.onChangeVillageSize, new Action<VillageSettingUIItem, VILLAGE_SIZE>(OnChangeVillageSize));
	}

	private void OnDisable()
	{
		VillageSettingUIItem.onClickMinus = (Action<VillageSettingUIItem>)Delegate.Remove(VillageSettingUIItem.onClickMinus, new Action<VillageSettingUIItem>(OnClickMinus));
		VillageSettingUIItem.onChangeName = (Action<VillageSettingUIItem, string>)Delegate.Remove(VillageSettingUIItem.onChangeName, new Action<VillageSettingUIItem, string>(OnChangeFactionName));
		VillageSettingUIItem.onClickRandomizeName = (Action<VillageSettingUIItem>)Delegate.Remove(VillageSettingUIItem.onClickRandomizeName, new Action<VillageSettingUIItem>(OnClickRandomizeName));
		VillageSettingUIItem.onChangeVillageSize = (Action<VillageSettingUIItem, VILLAGE_SIZE>)Delegate.Remove(VillageSettingUIItem.onChangeVillageSize, new Action<VillageSettingUIItem, VILLAGE_SIZE>(OnChangeVillageSize));
	}

	private void OnDestroy()
	{
		m_factionSettingVillageEditorUIView?.Unsubscribe(this);
	}

	private void OnClickMinus(VillageSettingUIItem p_item)
	{
		int index = p_item.transform.GetSiblingIndex();
		_currentlyEditingFaction.villageSettings.RemoveAt(index);
		UpdateVillageItems();
	}

	private void OnChangeFactionName(VillageSettingUIItem p_item, string p_newName)
	{
		int index = p_item.transform.GetSiblingIndex();
		VillageSetting villageSetting = _currentlyEditingFaction.villageSettings[index];
		villageSetting.villageName = p_newName;
		_currentlyEditingFaction.villageSettings[index] = villageSetting;
		p_item.SetItemDetails(villageSetting);
	}

	private void OnClickRandomizeName(VillageSettingUIItem p_item)
	{
		int index = p_item.transform.GetSiblingIndex();
		VillageSetting villageSetting = _currentlyEditingFaction.villageSettings[index];
		villageSetting.villageName = RandomNameGenerator.GenerateSettlementName(RACE.HUMANS);
		_currentlyEditingFaction.villageSettings[index] = villageSetting;
		p_item.SetItemDetails(villageSetting);
	}

	private void OnChangeVillageSize(VillageSettingUIItem p_item, VILLAGE_SIZE p_villageSize)
	{
		int index = p_item.transform.GetSiblingIndex();
		VillageSetting villageSetting = _currentlyEditingFaction.villageSettings[index];
		villageSetting.villageSize = p_villageSize;
		_currentlyEditingFaction.villageSettings[index] = villageSetting;
		p_item.SetItemDetails(villageSetting);
	}

	private void UpdateVillageItems()
	{
		m_factionSettingVillageEditorUIView.UpdateVillageItems(_currentlyEditingFaction.villageSettings);
		UpdateAddVillageBtn();
	}

	private void UpdateAddVillageBtn()
	{
		int num = 0;
		for (int i = 0; i < m_factionSettingVillageEditorUIView.UIModel.villageSettingUIItems.Length; i++)
		{
			if (m_factionSettingVillageEditorUIView.UIModel.villageSettingUIItems[i].gameObject.activeSelf)
			{
				num++;
			}
		}
		bool flag = num >= WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingVillages();
		m_factionSettingVillageEditorUIView.SetAddVillageBtnState(!flag);
	}

	public void OnClickAddVillage()
	{
		_currentlyEditingFaction.AddVillageSetting(VillageSetting.Default);
		UpdateVillageItems();
		UpdateAddVillageBtn();
	}

	public void OnClickClose()
	{
		HideUI();
		onHideAction?.Invoke();
	}
}
