using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class SchemeUIController : MVCUIController, SchemeUIView.IListener
{
	[SerializeField]
	private SchemeUIModel m_schemeUIModel;

	private SchemeUIView m_schemeUIView;

	public BlackmailUIController blackmailUIController;

	public TemptUIController temptUIController;

	private Character _targetCharacter;

	private object _otherTarget;

	private SchemeData _schemeUsed;

	private float _successRate;

	private List<SchemeUIItem> _schemeUIItems;

	private List<IIntel> _chosenBlackmail;

	private List<TEMPTATION> _chosenTemptations;

	private Action _onCloseAction;

	private void Start()
	{
		InstantiateUI();
		HideUI();
	}

	private void Awake()
	{
		_chosenBlackmail = new List<IIntel>();
		_chosenTemptations = new List<TEMPTATION>();
	}

	private void OnDestroy()
	{
		m_schemeUIView?.Unsubscribe(this);
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SchemeUIView.Create(_canvas, m_schemeUIModel, delegate(SchemeUIView p_ui)
		{
			m_schemeUIView = p_ui;
			m_schemeUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			blackmailUIController.InstantiateUI();
			blackmailUIController.HideUI();
			temptUIController.InstantiateUI();
			temptUIController.HideUI();
		});
	}

	public void Show(Character p_targetCharacter, object p_otherTarget, SchemeData p_schemeUsed, Action p_onCloseAction)
	{
		ShowUI();
		_targetCharacter = p_targetCharacter;
		_otherTarget = p_otherTarget;
		_schemeUsed = p_schemeUsed;
		if (_schemeUIItems == null)
		{
			_schemeUIItems = new List<SchemeUIItem>();
		}
		_onCloseAction = p_onCloseAction;
		_chosenBlackmail.Clear();
		_chosenTemptations.Clear();
		ClearSchemeUIItems();
		m_schemeUIView.SetTitle(_schemeUsed.localizedName);
		m_schemeUIView.SetBlackmailBtnInteractableState(HasValidBlackmailForTarget(p_targetCharacter));
		m_schemeUIView.SetTemptBtnInteractableState(temptUIController.HasValidTemptationsForTarget(p_targetCharacter));
		if (_targetCharacter.traitContainer.HasTrait("Demon Cultist"))
		{
			float num = 100f;
			float rate = num;
			ProcessSchemeSuccessRateWithMultipliers(ref rate);
			CreateAndAddNewSchemeUIItem(LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Demon Cultist"), rate, num, null, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem).btnMinus.interactable = false;
			UpdateSuccessRate();
		}
	}

	public override void HideUI()
	{
		base.HideUI();
		_onCloseAction?.Invoke();
	}

	private void AddBlackmail(IIntel p_intel)
	{
		BLACKMAIL_TYPE blackMailTypeConsideringTarget = p_intel.GetBlackMailTypeConsideringTarget(_targetCharacter);
		if (blackMailTypeConsideringTarget == BLACKMAIL_TYPE.None)
		{
			CreateAndAddNewSchemeUIItem(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Non_Blackmail"), 0f, 0f, delegate(SchemeUIItem item)
			{
				OnClickMinusSchemeUIItem(item);
				_chosenBlackmail.Remove(p_intel);
			}, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
		}
		else
		{
			float schemeSuccessRate = GetSchemeSuccessRate(blackMailTypeConsideringTarget);
			float baseSchemeSuccessRate = GetBaseSchemeSuccessRate(blackMailTypeConsideringTarget);
			CreateAndAddNewSchemeUIItem(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", blackMailTypeConsideringTarget.ToStringEnum() + "_Blackmail"), schemeSuccessRate, baseSchemeSuccessRate, delegate(SchemeUIItem item)
			{
				OnClickMinusSchemeUIItem(item);
				_chosenBlackmail.Remove(p_intel);
			}, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
		}
		_chosenBlackmail.Add(p_intel);
		UpdateSuccessRate();
	}

	private List<IIntel> GetValidBlackmailForTarget(Character p_target)
	{
		List<IIntel> list = null;
		for (int i = 0; i < PlayerManager.Instance.player.allIntel.Count; i++)
		{
			IIntel intel = PlayerManager.Instance.player.allIntel[i];
			if (intel.CanBeUsedToBlackmailCharacter(p_target))
			{
				if (list == null)
				{
					list = new List<IIntel>();
				}
				list.Add(intel);
			}
		}
		return list;
	}

	private bool HasValidBlackmailForTarget(Character p_target)
	{
		for (int i = 0; i < PlayerManager.Instance.player.allIntel.Count; i++)
		{
			if (PlayerManager.Instance.player.allIntel[i].CanBeUsedToBlackmailCharacter(p_target))
			{
				return true;
			}
		}
		return false;
	}

	private void AddTemptation(TEMPTATION p_temptation)
	{
		float schemeSuccessRate = GetSchemeSuccessRate(p_temptation);
		float baseSchemeSuccessRate = GetBaseSchemeSuccessRate(p_temptation);
		CreateAndAddNewSchemeUIItem(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", p_temptation.ToStringEnumWithSpace()), schemeSuccessRate, baseSchemeSuccessRate, delegate(SchemeUIItem item)
		{
			OnClickMinusSchemeUIItem(item);
			_chosenTemptations.Remove(p_temptation);
		}, OnHoverEnterSchemeUIItem, OnHoverExitSchemeUIItem);
		_chosenTemptations.Add(p_temptation);
		UpdateSuccessRate();
	}

	private void ActivateTemptationEffect(TEMPTATION p_temptation)
	{
		switch (p_temptation)
		{
		case TEMPTATION.Dark_Blessing:
			_targetCharacter.traitContainer.AddTrait(_targetCharacter, "Dark Blessing");
			break;
		case TEMPTATION.Empower:
			_targetCharacter.traitContainer.AddTrait(_targetCharacter, "Mighty");
			break;
		case TEMPTATION.Cleanse_Flaws:
			_targetCharacter.traitContainer.RemoveAllTraitsByType(_targetCharacter, TRAIT_TYPE.FLAW);
			break;
		default:
			throw new ArgumentOutOfRangeException("p_temptation", p_temptation, null);
		}
	}

	private float GetSchemeSuccessRate(TEMPTATION temptationType)
	{
		float rate = GetBaseSchemeSuccessRate(temptationType);
		ProcessSchemeSuccessRateWithMultipliers(ref rate);
		return rate;
	}

	private float GetBaseSchemeSuccessRate(TEMPTATION temptationType)
	{
		float result = 0f;
		switch (temptationType)
		{
		case TEMPTATION.Dark_Blessing:
			result = 50f;
			break;
		case TEMPTATION.Empower:
			result = 25f;
			break;
		case TEMPTATION.Cleanse_Flaws:
		{
			int num = 0;
			for (int i = 0; i < _targetCharacter.traitContainer.traits.Count; i++)
			{
				if (_targetCharacter.traitContainer.traits[i].type == TRAIT_TYPE.FLAW)
				{
					num++;
				}
			}
			result = 20f * (float)num;
			break;
		}
		}
		return result;
	}

	private float GetSchemeSuccessRate(BLACKMAIL_TYPE blackmailType)
	{
		float rate = GetBaseSchemeSuccessRate(blackmailType);
		ProcessSchemeSuccessRateWithMultipliers(ref rate);
		return rate;
	}

	private float GetBaseSchemeSuccessRate(BLACKMAIL_TYPE blackmailType)
	{
		float result = 0f;
		switch (blackmailType)
		{
		case BLACKMAIL_TYPE.Strong:
			result = 50f;
			break;
		case BLACKMAIL_TYPE.Normal:
			result = 35f;
			break;
		case BLACKMAIL_TYPE.Weak:
			result = 20f;
			break;
		}
		return result;
	}

	private void ProcessSchemeSuccessRateWithMultipliers(ref float rate)
	{
		_schemeUsed.ProcessSuccessRateWithMultipliers(_targetCharacter, _otherTarget, ref rate);
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void OnClickConfirm()
	{
		HideUI();
		for (int i = 0; i < _chosenBlackmail.Count; i++)
		{
			IIntel intel = _chosenBlackmail[i];
			PlayerManager.Instance.player.RemoveIntel(intel);
		}
		bool flag = _schemeUsed.ShouldSchemeBeSuccessful(_targetCharacter, _otherTarget, _successRate);
		if (flag)
		{
			AudioManager.Instance.TryPlayUISFX("Play_Meddler_Success");
			Messenger.Broadcast(CharacterSignals.CHARACTER_MEDDLER_SCHEME_SUCCESSFUL, _targetCharacter);
			if (_chosenTemptations.Count > 0)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Schemes", "PlayerPowerAlerts_Table", "Scheme tempted", LOG_TAG.Player);
				string text = string.Empty;
				for (int j = 0; j < _chosenTemptations.Count; j++)
				{
					TEMPTATION tEMPTATION = _chosenTemptations[j];
					if (j == _chosenTemptations.Count - 1)
					{
						text = text + ", " + LocalizationManager.And + " ";
					}
					else if (j > 0)
					{
						text += ", ";
					}
					text += LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", tEMPTATION.ToStringEnumWithSpace());
					ActivateTemptationEffect(tEMPTATION);
				}
				log.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
		}
		else
		{
			AudioManager.Instance.TryPlayUISFX("Play_Meddler_Fail");
		}
		_schemeUsed.ProcessScheme(_targetCharacter, _otherTarget, flag);
	}

	public void OnClickBlackmail()
	{
		List<IIntel> validBlackmailForTarget = GetValidBlackmailForTarget(_targetCharacter);
		if (validBlackmailForTarget != null)
		{
			blackmailUIController.ShowBlackmailUI(validBlackmailForTarget, _chosenBlackmail, OnConfirmBlackmail);
		}
	}

	private void OnConfirmBlackmail(List<IIntel> p_chosenBlackmail)
	{
		for (int i = 0; i < p_chosenBlackmail.Count; i++)
		{
			IIntel intel = p_chosenBlackmail[i];
			if (!_chosenBlackmail.Contains(intel))
			{
				AddBlackmail(intel);
			}
		}
	}

	public void OnClickTemptation()
	{
		if (temptUIController.HasValidTemptationsForTarget(_targetCharacter))
		{
			temptUIController.ShowTemptationPopup(_targetCharacter, OnConfirmTemptation, _chosenTemptations);
		}
	}

	private void OnConfirmTemptation(List<TEMPTATION> p_temptations)
	{
		for (int i = 0; i < p_temptations.Count; i++)
		{
			TEMPTATION tEMPTATION = p_temptations[i];
			if (!_chosenTemptations.Contains(tEMPTATION))
			{
				AddTemptation(tEMPTATION);
			}
		}
		if (p_temptations.Count > 0)
		{
			Messenger.Broadcast(UISignals.TEMPTATIONS_OFFERED);
		}
	}

	public void OnHoverOverBlackmailBtn(UIHoverPosition p_hoverPos)
	{
		if (!HasValidBlackmailForTarget(_targetCharacter))
		{
			UIManager.Instance.ShowSmallInfo(Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Blackmail_Tooltip") + " " + _targetCharacter.name + "!"), p_hoverPos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Blackmail"));
		}
	}

	public void OnHoverOverTemptBtn(UIHoverPosition p_hoverPos)
	{
		if (!temptUIController.HasValidTemptationsForTarget(_targetCharacter))
		{
			UIManager.Instance.ShowSmallInfo(Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Temptations_Tooltip") + " " + _targetCharacter.name + "!"), p_hoverPos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Temptations"));
		}
	}

	public void OnHoverOutBlackmailBtn()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverOutTemptBtn()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverOverSuccessRate()
	{
		if (_schemeUIItems.Count <= 0)
		{
			return;
		}
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(_schemeUsed.type);
		float resistanceValue = _targetCharacter.piercingAndResistancesComponent.GetResistanceValue(scriptableObjPlayerSkillData.resistanceType);
		if (resistanceValue > 0f)
		{
			string empty = string.Empty;
			float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(_schemeUsed.type);
			float num = 0f;
			for (int i = 0; i < _schemeUIItems.Count; i++)
			{
				num += _schemeUIItems[i].successRate;
			}
			empty = empty + "Base Success Rate: " + num.ToString("N1") + "%";
			empty = empty + "\nPiercing: " + pierceBasedOnCurrentLevel.ToString("N2") + "%";
			empty = empty + "\nResistance: " + resistanceValue.ToString("N2") + "%";
			UIManager.Instance.ShowSmallInfo(empty);
		}
	}

	public void OnHoverOutSuccessRate()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void ClearSchemeUIItems()
	{
		for (int i = 0; i < _schemeUIItems.Count; i++)
		{
			SchemeUIItem schemeUIItem = _schemeUIItems[i];
			ObjectPoolManager.Instance.DestroyObject(schemeUIItem.gameObject);
		}
		_schemeUIItems.Clear();
		UpdateSuccessRate();
	}

	private void RemoveSchemeUIItem(SchemeUIItem item)
	{
		if (_schemeUIItems.Remove(item))
		{
			ObjectPoolManager.Instance.DestroyObject(item.gameObject);
			UpdateSuccessRate();
		}
	}

	private SchemeUIItem CreateAndAddNewSchemeUIItem(string p_text, float p_successRate, float p_baseSuccessRate, Action<SchemeUIItem> p_onClickMinusAction, Action<SchemeUIItem> p_onHoverEnterAction, Action<SchemeUIItem> p_onHoverExitAction)
	{
		SchemeUIItem component = UIManager.Instance.InstantiateUIObject(m_schemeUIView.UIModel.schemeUIItemPrefab.name, m_schemeUIView.UIModel.scrollViewSchemes.content).GetComponent<SchemeUIItem>();
		component.SetItemDetails(p_text, p_successRate, p_baseSuccessRate);
		component.SetClickMinusAction(p_onClickMinusAction);
		component.SetOnHoverEnterAction(p_onHoverEnterAction);
		component.SetOnHoverExitAction(p_onHoverExitAction);
		_schemeUIItems.Add(component);
		return component;
	}

	private void OnClickMinusSchemeUIItem(SchemeUIItem item)
	{
		RemoveSchemeUIItem(item);
	}

	private void OnHoverEnterSchemeUIItem(SchemeUIItem item)
	{
		string text = "<b><size=18>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Success Rate") + ": <color=green>+" + item.successRate.ToString("N1") + "%</color></size></b>";
		string text2 = LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", "Base") + ": <color=white>+" + item.baseSuccessRate.ToString("N1") + "%</color>";
		string successRateMultiplierText = _schemeUsed.GetSuccessRateMultiplierText(_targetCharacter, _otherTarget);
		string info = "<line-height=100%>" + text + "\n<line-height=70%>" + text2 + "\n<line-height=70%>" + successRateMultiplierText;
		UIManager.Instance.ShowSmallInfo(info);
	}

	private void OnHoverExitSchemeUIItem(SchemeUIItem item)
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void UpdateSuccessRate()
	{
		float p_value = 0f;
		for (int i = 0; i < _schemeUIItems.Count; i++)
		{
			p_value += _schemeUIItems[i].successRate;
		}
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(_schemeUsed.type);
		float resistanceValue = _targetCharacter.piercingAndResistancesComponent.GetResistanceValue(scriptableObjPlayerSkillData.resistanceType);
		CombatManager.ModifyValueByPiercingAndResistance(ref p_value, PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(_schemeUsed.type), resistanceValue);
		_successRate = p_value;
		float value = p_value;
		value = Mathf.Clamp(value, 0f, 100f);
		m_schemeUIView.SetSuccessRate("<color=green>+" + value.ToString("N1") + "%</color>");
	}
}
