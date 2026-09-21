using System;
using UnityEngine.Serialization;

[Serializable]
public class SaveDataCurrenciesComponent : SaveData<CurrenciesComponent>
{
	[FormerlySerializedAs("plaguePoints")]
	public int chaoticEnergy;

	[FormerlySerializedAs("maxPlaguePoints")]
	public int maxChaoticEnergy;

	public int mana;

	public int spiritEnergy;

	public override void Save(CurrenciesComponent p_component)
	{
		chaoticEnergy = p_component.chaoticEnergy;
		maxChaoticEnergy = p_component.maxChaoticEnergy;
		mana = p_component.mana;
		spiritEnergy = p_component.spiritEnergy;
	}

	public override CurrenciesComponent Load()
	{
		return new CurrenciesComponent(this);
	}
}
