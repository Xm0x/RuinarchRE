using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using UtilityScripts;

public class SpellItem : NameplateItem<SkillData>
{
	[SerializeField]
	private Image cooldownImage;

	[SerializeField]
	private Image skillImage;

	[SerializeField]
	private TextMeshProUGUI manaCostLbl;

	[SerializeField]
	private TextMeshProUGUI chargesLbl;

	[SerializeField]
	private MaxRectTransform maxRectTransform;

	private Func<SkillData, bool> _shouldBeInteractableChecker;

	public SkillData spellData { get; private set; }

	public override void SetObject(SkillData spellData)
	{
		base.SetObject(spellData);
		base.name = spellData.localizedName;
		button.name = spellData.localizedName;
		base.toggle.name = spellData.localizedName;
		this.spellData = spellData;
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spellData.type);
		skillImage.sprite = scriptableObjPlayerSkillData.skillIcon;
		UpdateAllData();
		Messenger.AddListener<SkillData>(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.UPDATE_PLAYER_SKILL, OnSpellUpdated);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnExecuteSpell);
		Messenger.AddListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, OnBonusChargesAdjusted);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SKILL_ENABLED, OnSkillEnabled);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SKILL_DISABLED, OnSkillDisabled);
		if (spellData.hasSpiritEnergyCost)
		{
			Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnPlayerAdjustedSpiritEnergy);
		}
		LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
		SetAsDefault();
		UpdateInteractableState();
	}

	private void OnLocaleChanged(Locale p_newLang)
	{
		UpdateAllData();
	}

	protected override void OnDestroy()
	{
		LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
		base.OnDestroy();
	}

	public void UpdateAllData()
	{
		UpdateNameText();
		UpdateManaText();
		UpdateChargesText();
	}

	private void UpdateManaText()
	{
		manaCostLbl.text = string.Empty;
		if (spellData.manaCost > 0)
		{
			manaCostLbl.text += $"{Utilities.ManaIcon()}{spellData.manaCost} ";
		}
		else if (spellData.spiritEnergyCost > 0)
		{
			manaCostLbl.text += $"{Utilities.SpiritEnergyIcon()}{spellData.spiritEnergyCost} ";
		}
	}

	private void UpdateChargesText()
	{
		chargesLbl.text = spellData.GetChargesCombinedIconFirstUIText() + "  ";
	}

	public void UpdateNameText()
	{
		mainLbl.text = spellData.localizedName;
		if (!spellData.hasRemainingOrUnliChaosOrbs || spellData.type == PLAYER_SKILL_TYPE.BRAINWASH)
		{
			mainLbl.color = PlayerSkillManager.Instance.withoutChaosOrbsTextColor;
		}
		else
		{
			mainLbl.color = PlayerSkillManager.Instance.withChaosOrbsTextColor;
		}
		if (mainLbl.rectTransform.sizeDelta.x >= maxRectTransform.maxX && !LocalizationSettings.SelectedLocale.LocaleName.Equals("Vietnamese (vi)"))
		{
			string text = mainLbl.text;
			mainLbl.text = text.Replace(' ', '\n');
		}
	}

	private void OnPlayerNoActiveSpell(SkillData spellData)
	{
		if (this.spellData == spellData)
		{
			UpdateInteractableState();
			if (_toggle.isOn)
			{
				_toggle.isOn = false;
			}
		}
	}

	private void OnSpellCooldownStarted(SkillData spellData)
	{
		if (this.spellData == spellData)
		{
			UpdateInteractableState();
			if (spellData is MinionPlayerSkill)
			{
				SetCooldownState(spellData.isInCooldown);
				StartCooldownFill();
			}
			else
			{
				SetCooldownState(spellData.isInCooldown);
				StartCooldownFill();
			}
		}
	}

	private void OnSpellUpdated(SkillData p_upgradedSkill)
	{
		if (spellData.type == p_upgradedSkill.type)
		{
			UpdateManaText();
			UpdateChargesText();
		}
	}

	private void OnSpellCooldownFinished(SkillData spellData)
	{
		if (this.spellData == spellData)
		{
			SetCooldownState(spellData.isInCooldown);
			StopCooldownFill();
		}
	}

	private void OnExecuteSpell(SkillData spellData)
	{
		if (this.spellData == spellData)
		{
			UpdateChargesText();
			UpdateInteractableState();
		}
	}

	private void OnChargesAdjusted(SkillData spellData, int p_amount)
	{
		if (this.spellData == spellData)
		{
			UpdateChargesText();
			UpdateInteractableState();
		}
	}

	private void OnBonusChargesAdjusted(SkillData spellData)
	{
		if (this.spellData == spellData)
		{
			UpdateChargesText();
			UpdateInteractableState();
		}
	}

	private void OnPlayerAdjustedMana(int adjusted, int mana)
	{
		UpdateInteractableState();
	}

	private void OnPlayerAdjustedSpiritEnergy(int p_adjustedAmount, int p_totalSpiritEnergy)
	{
		UpdateInteractableState();
	}

	private void OnSkillDisabled(SkillData p_skill)
	{
		if (p_skill == spellData)
		{
			UpdateInteractableState();
		}
	}

	private void OnSkillEnabled(SkillData p_skill)
	{
		if (p_skill == spellData)
		{
			UpdateInteractableState();
		}
	}

	private void SetAsDefault()
	{
		SetAsToggle();
		ClearAllHoverEnterActions();
		ClearAllHoverExitActions();
		AddHoverEnterAction(delegate(SkillData spellData)
		{
			PlayerUI.Instance.OnHoverSpell(spellData, PlayerUI.Instance.spellListHoverPosition);
		});
		AddHoverExitAction(delegate(SkillData spellData)
		{
			PlayerUI.Instance.OnHoverOutSpell(spellData);
		});
	}

	private void SetCooldownState(bool state)
	{
		cooldownImage.gameObject.SetActive(state);
	}

	public void ForceUpdateInteractableState()
	{
		UpdateInteractableState();
	}

	private void UpdateInteractableState()
	{
		SetInteractableState(_shouldBeInteractableChecker?.Invoke(spellData) ?? (spellData != null && spellData.CanPerformAbility() && spellData.IsValid()));
	}

	public void OnToggleSpell(bool state)
	{
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		if (PlayerManager.Instance.player.currentActivePlayerSpell != null)
		{
			PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		}
		if (state)
		{
			PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(spellData);
		}
	}

	public void SetInteractableChecker(Func<SkillData, bool> p_checker)
	{
		_shouldBeInteractableChecker = p_checker;
	}

	public void UpdateCooldownFromLastState()
	{
		SetCooldownState(spellData.isInCooldown);
		if (cooldownImage.gameObject.activeSelf)
		{
			float fillAmount = 1f - (float)spellData.currentCooldownTick / (float)spellData.cooldown;
			cooldownImage.fillAmount = fillAmount;
		}
	}

	private void StartCooldownFill()
	{
		cooldownImage.fillAmount = 1f;
		PerTickCooldown();
		Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
	}

	private void PerTickCooldown()
	{
		float endValue = 1f - (float)spellData.currentCooldownTick / (float)spellData.cooldown;
		cooldownImage.DOFillAmount(endValue, 0.4f);
	}

	private void StopCooldownFill()
	{
		cooldownImage.fillAmount = 1f;
		UpdateInteractableState();
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
	}

	public override void Reset()
	{
		base.Reset();
		button.name = "Button";
		base.toggle.name = "Toggle";
		SetInteractableState(state: true);
		SetCooldownState(state: false);
		spellData = null;
		cooldownImage.fillAmount = 1f;
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnExecuteSpell);
		Messenger.RemoveListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, OnBonusChargesAdjusted);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.UPDATE_PLAYER_SKILL, OnSpellUpdated);
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SKILL_ENABLED, OnSkillEnabled);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SKILL_DISABLED, OnSkillDisabled);
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnPlayerAdjustedSpiritEnergy);
	}
}
