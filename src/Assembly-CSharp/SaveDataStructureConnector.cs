using Inner_Maps.Location_Structures;

public class SaveDataStructureConnector : SaveData<StructureConnector>
{
	public bool isOpen;

	public bool isPartOfLocationStructureObject;

	public override void Save(StructureConnector data)
	{
		base.Save(data);
		isOpen = data.isOpen;
		isPartOfLocationStructureObject = data.isPartOfLocationStructureObject;
	}
}
