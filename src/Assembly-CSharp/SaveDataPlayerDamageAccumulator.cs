using System;

[Serializable]
public class SaveDataPlayerDamageAccumulator : SaveData<PlayerDamageAccumulator>
{
	public int accumulatedDamage;

	public bool activatedSpellDamageChaosOrbPassiveSkill;

	public override void Save(PlayerDamageAccumulator data)
	{
		base.Save(data);
		accumulatedDamage = data.accumulatedDamage;
		activatedSpellDamageChaosOrbPassiveSkill = data.activatedSpellDamageChaosOrbPassiveSkill;
	}

	public override PlayerDamageAccumulator Load()
	{
		return new PlayerDamageAccumulator(this);
	}
}
