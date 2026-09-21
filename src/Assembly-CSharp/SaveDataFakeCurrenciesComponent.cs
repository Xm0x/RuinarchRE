using System;

[Serializable]
public class SaveDataFakeCurrenciesComponent : SaveData<FakeCurrenciesComponent>
{
	public int mana;

	public int chaoticEnergy;

	public int spirits;

	public override void Save(FakeCurrenciesComponent p_component)
	{
		mana = p_component.Mana;
		chaoticEnergy = p_component.ChaoticEnergy;
		spirits = p_component.Spirits;
	}

	public override FakeCurrenciesComponent Load()
	{
		return new FakeCurrenciesComponent(this);
	}
}
