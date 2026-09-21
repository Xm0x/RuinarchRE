using UtilityScripts;

public class SaveDataSummonMeterComponent : SaveData<SummonMeterComponent>
{
	public RuinarchBasicProgress progress;

	public override void Save(SummonMeterComponent data)
	{
		base.Save(data);
		progress = data.progress;
	}

	public override SummonMeterComponent Load()
	{
		return new SummonMeterComponent(this);
	}
}
