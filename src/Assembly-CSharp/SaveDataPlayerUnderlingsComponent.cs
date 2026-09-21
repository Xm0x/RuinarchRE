using System.Collections.Generic;

public class SaveDataPlayerUnderlingsComponent : SaveData<PlayerUnderlingsComponent>
{
	public Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> monsterUnderlingCharges;

	public Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges> demonUnderlingCharges;

	public string persistentDefendParty;

	public int cooldown;

	public override void Save(PlayerUnderlingsComponent data)
	{
		base.Save(data);
		monsterUnderlingCharges = new Dictionary<SUMMON_TYPE, MonsterAndDemonUnderlingCharges>();
		foreach (KeyValuePair<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> monsterUnderlingCharge in data.monsterUnderlingCharges)
		{
			monsterUnderlingCharges.Add(monsterUnderlingCharge.Key, monsterUnderlingCharge.Value);
		}
		demonUnderlingCharges = new Dictionary<MINION_TYPE, MonsterAndDemonUnderlingCharges>();
		foreach (KeyValuePair<MINION_TYPE, MonsterAndDemonUnderlingCharges> demonUnderlingCharge in data.demonUnderlingCharges)
		{
			demonUnderlingCharges.Add(demonUnderlingCharge.Key, demonUnderlingCharge.Value);
		}
		persistentDefendParty = data.persistentDefendParty?.persistentID;
		cooldown = data.cooldown;
	}

	public override PlayerUnderlingsComponent Load()
	{
		return new PlayerUnderlingsComponent(this);
	}
}
