using System;
using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;

public class SchemeUIItem : PooledObject
{
	private Action<SchemeUIItem> onClickMinusAction;

	private Action<SchemeUIItem> onHoverEnterAction;

	private Action<SchemeUIItem> onHoverExitAction;

	public TextMeshProUGUI txtName;

	public TextMeshProUGUI txtSucessRate;

	public RuinarchButton btnMinus;

	private float _successRate;

	private float _baseSuccessRate;

	public float successRate => _successRate;

	public float baseSuccessRate => _baseSuccessRate;

	private void OnEnable()
	{
		btnMinus.onClick.AddListener(OnClickMinus);
	}

	private void OnDisable()
	{
		btnMinus.onClick.RemoveListener(OnClickMinus);
	}

	public void SetItemDetails(string p_text, float p_successRate, float p_baseSuccessRate)
	{
		_successRate = p_successRate;
		_baseSuccessRate = p_baseSuccessRate;
		txtName.text = p_text;
		txtSucessRate.text = "<color=green>+" + p_successRate.ToString("N1") + "%</color>";
	}

	public void SetClickMinusAction(Action<SchemeUIItem> action)
	{
		onClickMinusAction = action;
	}

	public void SetOnHoverEnterAction(Action<SchemeUIItem> action)
	{
		onHoverEnterAction = action;
	}

	public void SetOnHoverExitAction(Action<SchemeUIItem> action)
	{
		onHoverExitAction = action;
	}

	private void OnClickMinus()
	{
		onClickMinusAction?.Invoke(this);
	}

	public void OnHoverEnter()
	{
		onHoverEnterAction?.Invoke(this);
	}

	public void OnHoverExit()
	{
		onHoverExitAction?.Invoke(this);
	}

	public override void Reset()
	{
		base.Reset();
		onClickMinusAction = null;
		onHoverEnterAction = null;
		onHoverExitAction = null;
		txtName.text = string.Empty;
		txtSucessRate.text = string.Empty;
	}
}
