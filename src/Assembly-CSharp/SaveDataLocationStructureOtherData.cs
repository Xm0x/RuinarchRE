public class SaveDataLocationStructureOtherData : SaveDataOtherData
{
	public string structureID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		LocationStructureOtherData locationStructureOtherData = data as LocationStructureOtherData;
		structureID = locationStructureOtherData.locationStructure.persistentID;
	}

	public override OtherData Load()
	{
		return new LocationStructureOtherData(this);
	}
}
