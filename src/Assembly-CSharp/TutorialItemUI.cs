using System;
using DG.Tweening;
using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class TutorialItemUI : PooledObject
{
	public static Action<TutorialManager.Tutorial_Type> onTutorialItemToggledOn;

	public static Action<TutorialManager.Tutorial_Type> onTutorialItemToggledOff;

	[SerializeField]
	private TextMeshProUGUI tutorialName;

	[SerializeField]
	private GameObject unreadTutorialGO;

	[SerializeField]
	private RuinarchToggle toggleMain;

	private TutorialManager.Tutorial_Type _tutorialType;

	public RuinarchToggle toggle => toggleMain;

	public TutorialManager.Tutorial_Type tutorialType => _tutorialType;

	private void Awake()
	{
		toggleMain.onValueChanged.AddListener(OnClickItem);
	}

	private void OnDisable()
	{
		unreadTutorialGO.transform.DOKill();
	}

	private void OnEnable()
	{
	}

	public void Initialize(TutorialManager.Tutorial_Type p_type, ToggleGroup p_group)
	{
		_tutorialType = p_type;
		TutorialScriptableObjectData tutorialData = TutorialManager.Instance.GetTutorialData(p_type);
		tutorialName.text = LocalizationManager.Instance.GetLocalizedValue("Tutorials_Table", tutorialData.tutorialNameKey);
		toggleMain.group = p_group;
		UpdateReadObject();
		Messenger.AddListener<TutorialManager.Tutorial_Type>(TutorialSignals.TUTORIAL_READ, OnTutorialRead);
	}

	private void OnTutorialRead(TutorialManager.Tutorial_Type p_type)
	{
		if (_tutorialType == p_type)
		{
			UpdateReadObject();
		}
	}

	private void UpdateReadObject()
	{
		if (SaveManager.Instance != null)
		{
			unreadTutorialGO.SetActive(!SaveManager.Instance.savePlayerManager.currentSaveDataPlayer.HasTutorialBeenRead(_tutorialType));
		}
	}

	private void OnClickItem(bool p_isOn)
	{
		if (p_isOn)
		{
			onTutorialItemToggledOn?.Invoke(_tutorialType);
		}
		else
		{
			onTutorialItemToggledOff?.Invoke(_tutorialType);
		}
	}

	public void ManualSelect()
	{
		toggle.SetIsOnWithoutNotify(value: true);
		toggle.group.NotifyToggleOn(toggle, sendCallback: false);
		onTutorialItemToggledOn?.Invoke(_tutorialType);
	}

	public override void Reset()
	{
		base.Reset();
		Messenger.RemoveListener<TutorialManager.Tutorial_Type>(TutorialSignals.TUTORIAL_READ, OnTutorialRead);
	}
}
