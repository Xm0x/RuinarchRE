using System;

[Serializable]
public class SaveDataThreatComponent : SaveData<ThreatComponent>
{
	public int threat;

	public override void Save(ThreatComponent component)
	{
		threat = component.threat;
	}

	public override ThreatComponent Load()
	{
		ThreatComponent threatComponent = new ThreatComponent();
		threatComponent.SetThreatFromSave(threat);
		return threatComponent;
	}
}
