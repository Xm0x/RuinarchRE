using System;
using System.Collections.Generic;
using Maccima_Games.Util;
using Ruinarch.MVCFramework;
using UnityEngine;

public class TemptUIController : MVCUIController, TemptUIView.IListener
{
	[SerializeField]
	private TemptUIModel m_temptUIModel;

	private TemptUIView m_temptUIView;

	private List<TEMPTATION> _chosenTemptations;

	private Action<List<TEMPTATION>> _onConfirmAction;

	private Character _targetCharacter;

	private void Awake()
	{
		_chosenTemptations = new List<TEMPTATION>();
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		TemptUIView.Create(_canvas, m_temptUIModel, delegate(TemptUIView p_ui)
		{
			m_temptUIView = p_ui;
			m_temptUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	public override void ShowUI()
	{
		base.ShowUI();
		_chosenTemptations.Clear();
	}

	private void OnDestroy()
	{
		m_temptUIView?.Unsubscribe(this);
	}

	public bool HasValidTemptationsForTarget(Character p_target)
	{
		for (int i = 0; i < m_temptUIView.allTemptationTypes.Length; i++)
		{
			if (m_temptUIView.allTemptationTypes[i].CanTemptCharacter(p_target))
			{
				return true;
			}
		}
		return false;
	}

	public void ShowTemptationPopup(Character p_target, Action<List<TEMPTATION>> p_onConfirmAction, List<TEMPTATION> p_alreadyChosenTemptations)
	{
		_targetCharacter = p_target;
		ShowUI();
		m_temptUIView.UpdateShownItems(p_target, p_alreadyChosenTemptations);
		_onConfirmAction = p_onConfirmAction;
		Messenger.Broadcast(UISignals.TEMPTATIONS_POPUP_SHOWN);
	}

	public void OnToggleDarkBlessing(bool p_isOn)
	{
		if (p_isOn)
		{
			_chosenTemptations.Add(TEMPTATION.Dark_Blessing);
		}
		else
		{
			_chosenTemptations.Remove(TEMPTATION.Dark_Blessing);
		}
	}

	public void OnToggleEmpower(bool p_isOn)
	{
		if (p_isOn)
		{
			_chosenTemptations.Add(TEMPTATION.Empower);
		}
		else
		{
			_chosenTemptations.Remove(TEMPTATION.Empower);
		}
	}

	public void OnToggleCleanseFlaws(bool p_isOn)
	{
		if (p_isOn)
		{
			_chosenTemptations.Add(TEMPTATION.Cleanse_Flaws);
		}
		else
		{
			_chosenTemptations.Remove(TEMPTATION.Cleanse_Flaws);
		}
	}

	public void OnHoverDarkBlessing()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("targetName", _targetCharacter.visuals.GetCharacterNameWithIconAndColor());
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Dark_Blessing_Tooltip", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowSmallInfo(localizedValue, "", autoReplaceText: false);
	}

	public void OnHoverEmpower()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("targetName", _targetCharacter.visuals.GetCharacterNameWithIconAndColor());
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Empower_Tooltip", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowSmallInfo(localizedValue, "", autoReplaceText: false);
	}

	public void OnHoverCleanseFlaws()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("targetName", _targetCharacter.visuals.GetCharacterNameWithIconAndColor());
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cleanse_Flaws_Tooltip", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowSmallInfo(localizedValue, "", autoReplaceText: false);
	}

	public void OnHoverOutTemptation()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void OnClickConfirm()
	{
		HideUI();
		_onConfirmAction?.Invoke(_chosenTemptations);
	}
}
