using System;
using EZObjectPools;
using Locations.Settlements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogVillageFilterItem : PooledObject
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private TextMeshProUGUI itemLbl;

	private NPCSettlement _settlement;

	private Action<bool, NPCSettlement> _onToggleAction;

	public bool isOn => toggle.isOn;

	public string settlementPersistentID => _settlement.persistentID;

	private void Awake()
	{
		toggle.onValueChanged.AddListener(OnFilterToggled);
	}

	public void Initialize(NPCSettlement p_settlement)
	{
		_settlement = p_settlement;
		itemLbl.text = p_settlement.iconRichText + " " + p_settlement.name;
		Messenger.AddListener<BaseSettlement>(SettlementSignals.SETTLEMENT_CHANGED_NAME, OnSettlementChangedName);
	}

	public void SetOnToggleAction(Action<bool, NPCSettlement> onToggleAction)
	{
		_onToggleAction = onToggleAction;
	}

	private void OnFilterToggled(bool p_isOn)
	{
		_onToggleAction?.Invoke(p_isOn, _settlement);
	}

	public void SetIsOnWithoutNotify(bool state)
	{
		toggle.SetIsOnWithoutNotify(state);
	}

	public void SetIsOn(bool state)
	{
		toggle.isOn = state;
	}

	public override void Reset()
	{
		base.Reset();
		_onToggleAction = null;
		Messenger.RemoveListener<BaseSettlement>(SettlementSignals.SETTLEMENT_CHANGED_NAME, OnSettlementChangedName);
	}

	private void OnSettlementChangedName(BaseSettlement p_settlement)
	{
		if (p_settlement == _settlement)
		{
			itemLbl.text = p_settlement.iconRichText + " " + p_settlement.name;
		}
	}
}
