using System;
using System.Linq;
using DG.Tweening;
using Inner_Maps.Location_Structures;
using Ruinarch;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;

public class ChooseSkillsPopup : MonoBehaviour
{
	[Header("Choose skills")]
	[SerializeField]
	private GameObject goChooseSkill;

	[SerializeField]
	private TextMeshProUGUI lblChooseSkill;

	[SerializeField]
	private CanvasGroup canvasGroupChooseSkill;

	[SerializeField]
	private ChooseSkillItemUI[] chooseSkillItems;

	[SerializeField]
	private RuinarchButton btnClose;

	private PortalUpgradeItem _currentlyClickedUpgradeItem;

	private PortalUpgradeTier _parentTier;

	private Sequence _showChooseSkillsSequence;

	private void Awake()
	{
		AssignChooseItemActions(OnClickChoosePower, OnHoverOverChoosePower, OnHoverOutChoosePower);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public void Show(PortalUpgradeItem p_item, PortalUpgradeTier p_parentTier)
	{
		if (p_item.chosenPowerForUpgrade == PLAYER_SKILL_TYPE.NONE)
		{
			lblChooseSkill.text = string.Format("{0} ({1}/{2})", LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Choose One:"), UIManager.Instance.currentUpgradeIndex, UIManager.Instance.totalUpgradesToShow);
			_currentlyClickedUpgradeItem = p_item;
			_parentTier = p_parentTier;
			PLAYER_SKILL_TYPE[] powerChoices = p_item.powerChoices;
			ShowSkillsToChoose(powerChoices, p_item);
			UIManager.Instance.Pause();
			UIManager.Instance.SetSpeedTogglesState(state: false);
			InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
			InnerMapCameraMove.Instance.DisableMovement();
		}
	}

	private void OnClickChoosePower(PLAYER_SKILL_TYPE p_skillType)
	{
		ThePortal obj = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		bool p_addChargesIfLearned = PlayerSkillLoadout.All_Common_Structures.Contains(p_skillType);
		obj.GainPowerFromPortalUpgrade(p_skillType, p_addChargesIfLearned);
		PlayerManager.Instance.player.playerSkillComponent.SetChosenPowerForUpgradeItem(_parentTier, _currentlyClickedUpgradeItem, p_skillType);
		Hide();
	}

	private void OnHoverOverChoosePower(PlayerSkillData p_skillData, ChooseSkillItemUI p_itemUI)
	{
		if (!p_itemUI.btnSkill.interactable)
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Already_Learned_Power"));
		}
	}

	private void OnHoverOutChoosePower(PlayerSkillData p_skillData, ChooseSkillItemUI p_itemUI)
	{
		if (!p_itemUI.btnSkill.interactable)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	private void Hide()
	{
		if (!UIManager.Instance.TryShowNextSkillPopup())
		{
			HideChooseSkillUI();
		}
	}

	private void AssignChooseItemActions(Action<PLAYER_SKILL_TYPE> p_onClick, Action<PlayerSkillData, ChooseSkillItemUI> p_onHoverOver, Action<PlayerSkillData, ChooseSkillItemUI> p_onHoverOut)
	{
		for (int i = 0; i < chooseSkillItems.Length; i++)
		{
			ChooseSkillItemUI obj = chooseSkillItems[i];
			obj.onButtonClick = (Action<PLAYER_SKILL_TYPE>)Delegate.Combine(obj.onButtonClick, p_onClick);
			obj.onHoverOver = (Action<PlayerSkillData, ChooseSkillItemUI>)Delegate.Combine(obj.onHoverOver, p_onHoverOver);
			obj.onHoverOut = (Action<PlayerSkillData, ChooseSkillItemUI>)Delegate.Combine(obj.onHoverOut, p_onHoverOut);
		}
	}

	private void ShowSkillsToChoose(PLAYER_SKILL_TYPE[] p_choices, PortalUpgradeItem p_upgradeItem)
	{
		for (int i = 0; i < chooseSkillItems.Length; i++)
		{
			ChooseSkillItemUI chooseSkillItemUI = chooseSkillItems[i];
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = p_choices[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				chooseSkillItemUI.InitItem(pLAYER_SKILL_TYPE);
				chooseSkillItemUI.gameObject.SetActive(value: true);
				if (PlayerManager.Instance.player.playerSkillComponent.CanPlayerChoosePowerFromOptionalPowers(pLAYER_SKILL_TYPE))
				{
					chooseSkillItemUI.EnableButton();
				}
				else
				{
					chooseSkillItemUI.DisableButton();
				}
			}
			else
			{
				chooseSkillItemUI.gameObject.SetActive(value: false);
			}
		}
		_showChooseSkillsSequence?.Kill();
		if (goChooseSkill.activeSelf)
		{
			canvasGroupChooseSkill.alpha = 1f;
			_showChooseSkillsSequence = DOTween.Sequence();
			for (int j = 0; j < chooseSkillItems.Length; j++)
			{
				ChooseSkillItemUI chooseSkillItemUI2 = chooseSkillItems[j];
				if (chooseSkillItemUI2.gameObject.activeSelf)
				{
					_showChooseSkillsSequence.Join(chooseSkillItemUI2.PrepareAnimation().SetDelay((float)j / 5f));
				}
			}
			_showChooseSkillsSequence.Play();
			return;
		}
		canvasGroupChooseSkill.alpha = 0f;
		goChooseSkill.SetActive(value: true);
		_showChooseSkillsSequence = DOTween.Sequence();
		_showChooseSkillsSequence.Join(canvasGroupChooseSkill.DOFade(1f, 0.5f));
		_showChooseSkillsSequence.AppendInterval(0.02f);
		for (int k = 0; k < chooseSkillItems.Length; k++)
		{
			ChooseSkillItemUI chooseSkillItemUI3 = chooseSkillItems[k];
			if (chooseSkillItemUI3.gameObject.activeSelf)
			{
				_showChooseSkillsSequence.Join(chooseSkillItemUI3.PrepareAnimation().SetDelay((float)k / 5f));
			}
		}
		_showChooseSkillsSequence.Play();
	}

	private void HideChooseSkillUI()
	{
		canvasGroupChooseSkill.alpha = 1f;
		Sequence sequence = DOTween.Sequence();
		sequence.Join(canvasGroupChooseSkill.DOFade(0f, 0.5f));
		sequence.OnComplete(delegate
		{
			goChooseSkill.SetActive(value: false);
		});
		sequence.Play();
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		InnerMapCameraMove.Instance.EnableMovement();
	}
}
