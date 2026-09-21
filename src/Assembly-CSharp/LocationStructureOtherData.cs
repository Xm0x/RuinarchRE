using Inner_Maps.Location_Structures;

public class LocationStructureOtherData : OtherData
{
	public LocationStructure locationStructure { get; private set; }

	public override object obj => locationStructure;

	public LocationStructureOtherData(LocationStructure locationStructure)
	{
		this.locationStructure = locationStructure;
	}

	public LocationStructureOtherData(SaveDataLocationStructureOtherData saveData)
	{
		locationStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveData.structureID);
	}

	public override SaveDataOtherData Save()
	{
		SaveDataLocationStructureOtherData saveDataLocationStructureOtherData = new SaveDataLocationStructureOtherData();
		saveDataLocationStructureOtherData.Save(this);
		return saveDataLocationStructureOtherData;
	}

	public override void CleanUp()
	{
		locationStructure = null;
	}
}
