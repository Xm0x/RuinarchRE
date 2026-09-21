using Inner_Maps.Location_Structures;

public class StructureSettingOtherData : OtherData
{
	public StructureSetting structureSetting { get; private set; }

	public override object obj => structureSetting;

	public StructureSettingOtherData(StructureSetting structureSetting)
	{
		this.structureSetting = structureSetting;
	}

	public StructureSettingOtherData(SaveDataStructureSettingOtherData saveData)
	{
		structureSetting = saveData.structureSetting;
	}

	public override SaveDataOtherData Save()
	{
		SaveDataStructureSettingOtherData saveDataStructureSettingOtherData = new SaveDataStructureSettingOtherData();
		saveDataStructureSettingOtherData.Save(this);
		return saveDataStructureSettingOtherData;
	}

	public override void CleanUp()
	{
		structureSetting = default(StructureSetting);
	}
}
