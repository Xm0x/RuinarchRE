using System;

[Serializable]
public class SaveDataPartyDamageAccumulator : SaveData<PartyDamageAccumulator>
{
	public int accumulatedDamageFromDemonRaid;

	public int accumulatedDamageFromMonsterSpawnerRaid;

	public override void Save(PartyDamageAccumulator data)
	{
		base.Save(data);
		accumulatedDamageFromDemonRaid = data.accumulatedDamageFromDemonRaid;
		accumulatedDamageFromMonsterSpawnerRaid = data.accumulatedDamageFromMonsterSpawnerRaid;
	}
}
