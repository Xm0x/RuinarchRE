using System;

[Serializable]
public class SaveDataTrapStructure : SaveData<TrapStructure>
{
	public string structure;

	public int duration;

	public int currentDuration;

	public string forcedStructure;

	public string forcedHex;

	public override void Save(TrapStructure data)
	{
		base.Save(data);
		duration = data.duration;
		currentDuration = data.currentDuration;
		if (data.structure != null)
		{
			structure = data.structure.persistentID;
		}
		if (data.forcedStructure != null)
		{
			forcedStructure = data.forcedStructure.persistentID;
		}
		if (data.forcedArea != null)
		{
			forcedHex = data.forcedArea.persistentID;
		}
	}

	public override TrapStructure Load()
	{
		return new TrapStructure(this);
	}
}
