using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UtilityScripts;

public class DecorationNameplateToggle : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _currenciesText;

	[SerializeField]
	private Toggle _toggle;

	private DecorationsData _data;

	private UnityAction<SkillData> _hoverEnterAction;

	private UnityAction<SkillData> _hoverExitAction;

	public Toggle toggle => _toggle;

	private void Start()
	{
		Messenger.AddListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnSkillChargesAdjusted);
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnSkillChargesAdjusted);
	}

	private void OnSkillChargesAdjusted(SkillData p_data, int p_amount)
	{
		if (p_data.type == PLAYER_SKILL_TYPE.DECORATIONS)
		{
			UpdateData();
		}
	}

	public void SetDecorationSkill(DecorationsData p_data)
	{
		_data = p_data;
		UpdateData();
	}

	public void UpdateData()
	{
		_currenciesText.text = string.Empty;
		if (_data.manaCost > 0)
		{
			_currenciesText.text += $"{Utilities.ManaIcon()}{_data.manaCost} ";
		}
		if (_data.spiritEnergyCost > 0)
		{
			_currenciesText.text += $"{Utilities.SpiritEnergyIcon()}{_data.spiritEnergyCost} ";
		}
		TextMeshProUGUI currenciesText = _currenciesText;
		currenciesText.text = currenciesText.text + _data.GetChargesCombinedIconFirstUIText() + "  ";
		if (_data.cooldown >= 0)
		{
			_currenciesText.text += $"{Utilities.CooldownIcon()}{GameManager.GetTimeAsWholeDuration(_data.cooldown)} {GameManager.GetTimeIdentifierAsWholeDuration(_data.cooldown)}";
		}
	}

	public void UpdateInteractability()
	{
		_toggle.interactable = _data.CanPerformAbility() && _data.IsValid();
	}

	public void SetHoverEnterAction(UnityAction<SkillData> p_action)
	{
		_hoverEnterAction = p_action;
	}

	public void SetHoverExitAction(UnityAction<SkillData> p_action)
	{
		_hoverExitAction = p_action;
	}

	public void OnHoverEnter()
	{
		_hoverEnterAction?.Invoke(_data);
	}

	public void OnHoverExit()
	{
		_hoverExitAction?.Invoke(_data);
	}
}
