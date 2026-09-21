using Inner_Maps.Location_Structures;

public class SaveDataStructureSettingOtherData : SaveDataOtherData
{
	public StructureSetting structureSetting;

	public override void Save(OtherData data)
	{
		base.Save(data);
		StructureSettingOtherData structureSettingOtherData = data as StructureSettingOtherData;
		structureSetting = structureSettingOtherData.structureSetting;
	}

	public override OtherData Load()
	{
		return new StructureSettingOtherData(this);
	}
}
