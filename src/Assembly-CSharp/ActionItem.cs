using DG.Tweening;
using EZObjectPools;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.UI;

public class ActionItem : PooledObject
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private Image actionImg;

	[SerializeField]
	private Image coverImg;

	[SerializeField]
	private Image cooldownCoverImg;

	[SerializeField]
	private Image highlightImg;

	[SerializeField]
	private TextMeshProUGUI actionLbl;

	[SerializeField]
	private UIHoverPosition _hoverPosition;

	private string expiryKey;

	public PlayerAction playerAction { get; private set; }

	public IPlayerActionTarget playerActionTarget { get; private set; }

	public void SetAction(PlayerAction playerAction, IPlayerActionTarget playerActionTarget)
	{
		base.name = playerAction.localizedName;
		this.playerAction = playerAction;
		this.playerActionTarget = playerActionTarget;
		SetHighlightState(p_state: false);
		actionImg.sprite = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(playerAction.type).actionItemIcon;
		actionLbl.text = playerAction.GetLabelName(playerActionTarget);
		base.gameObject.SetActive(value: true);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
		UpdateCooldownImage();
		ResetCooldown();
	}

	public void RefreshAction(PlayerAction playerAction, IPlayerActionTarget playerActionTarget)
	{
		base.name = playerAction.localizedName;
		this.playerAction = playerAction;
		this.playerActionTarget = playerActionTarget;
		SetHighlightState(p_state: false);
		actionImg.sprite = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(playerAction.type).actionItemIcon;
		actionLbl.text = playerAction.GetLabelName(playerActionTarget);
		base.gameObject.SetActive(value: true);
		UpdateCooldownImage();
	}

	public void SetInteractable(bool state)
	{
		button.interactable = state;
		coverImg.gameObject.SetActive(!state);
	}

	private void UpdateInteractableState()
	{
		SetInteractable(playerAction.CanPerformAbility());
	}

	private void UpdateCooldownImage()
	{
		cooldownCoverImg.gameObject.SetActive(playerAction.isInCooldown);
	}

	private void ResetCooldown()
	{
		cooldownCoverImg.fillAmount = 1f;
	}

	public void SetHighlightState(bool p_state)
	{
		highlightImg.gameObject.SetActive(p_state);
	}

	public void OnClickThis()
	{
		if (playerAction != null)
		{
			SetHighlightState(p_state: true);
			ITraitable traitable = playerActionTarget as ITraitable;
			playerAction.Activate(playerActionTarget, traitable?.traitContainer.HasTrait("Demon Cultist") ?? false);
		}
	}

	public void OnHoverEnter()
	{
		PlayerUI.Instance.OnHoverSpell(playerAction, _hoverPosition, playerActionTarget);
	}

	public void OnHoverExit()
	{
		PlayerUI.Instance.OnHoverOutSpell(playerAction);
	}

	private void OnSpellCooldownStarted(SkillData spellData)
	{
		if (playerAction == spellData)
		{
			SetCooldownState(state: true);
			StartCooldownFill();
		}
	}

	private void OnSpellCooldownFinished(SkillData spellData)
	{
		if (playerAction == spellData)
		{
			StopCooldownFill();
		}
	}

	public void ForceUpdateCooldown()
	{
		if (playerAction.isInCooldown)
		{
			OnSpellCooldownStarted(playerAction);
		}
		else
		{
			SetCooldownState(state: false);
		}
	}

	private void SetCooldownState(bool state)
	{
		SetInteractable(playerAction.CanPerformAbilityTo(playerActionTarget) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
		cooldownCoverImg.gameObject.SetActive(state);
	}

	private void StartCooldownFill()
	{
		cooldownCoverImg.fillAmount = 1f - (float)playerAction.currentCooldownTick / (float)playerAction.cooldown;
		Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
	}

	private void PerTickCooldown()
	{
		if (playerAction != null)
		{
			float endValue = 1f - (float)playerAction.currentCooldownTick / (float)playerAction.cooldown;
			cooldownCoverImg.DOFillAmount(endValue, 0.4f);
		}
	}

	private void StopCooldownFill()
	{
		SetCooldownState(state: false);
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
	}

	private void OnPlayerAdjustedMana(int adjusted, int mana)
	{
		UpdateInteractableState();
	}

	public override void Reset()
	{
		base.Reset();
		base.name = "Action Item";
		button.onClick.RemoveAllListeners();
		if (!string.IsNullOrEmpty(expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
		playerAction = null;
		DOTween.Kill(this);
		cooldownCoverImg.fillAmount = 0f;
		expiryKey = string.Empty;
		cooldownCoverImg.gameObject.SetActive(value: false);
		SetInteractable(state: true);
		SetHighlightState(p_state: false);
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
	}
}
