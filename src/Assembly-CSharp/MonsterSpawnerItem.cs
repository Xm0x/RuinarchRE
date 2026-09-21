using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class MonsterSpawnerItem : PooledObject
{
	public TextMeshProUGUI itemButtonText;

	public Toggle itemToggle;

	public SUMMON_TYPE monsterType { get; private set; }

	public void SetMonster(SUMMON_TYPE p_monsterType)
	{
		monsterType = p_monsterType;
		UpdateData();
		Messenger.AddListener<SUMMON_TYPE>(PlayerSignals.PLAYER_NO_ACTIVE_MONSTER, OnPlayerNoActiveMonster);
	}

	private void UpdateData()
	{
		itemButtonText.text = monsterType.ToStringEnumWithSpace();
	}

	private void OnPlayerNoActiveMonster(SUMMON_TYPE p_monsterType)
	{
		if (monsterType == p_monsterType && itemToggle.isOn)
		{
			itemToggle.isOn = false;
		}
	}

	public void OnToggleItem(bool state)
	{
		PlayerManager.Instance.player.SetCurrentlyActiveMonster(SUMMON_TYPE.None);
		if (state)
		{
			PlayerManager.Instance.player.SetCurrentlyActiveMonster(monsterType);
		}
	}

	public override void Reset()
	{
		base.Reset();
		monsterType = SUMMON_TYPE.None;
		Messenger.RemoveListener<SUMMON_TYPE>(PlayerSignals.PLAYER_NO_ACTIVE_MONSTER, OnPlayerNoActiveMonster);
	}
}
