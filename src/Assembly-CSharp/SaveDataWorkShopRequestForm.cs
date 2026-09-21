public class SaveDataWorkShopRequestForm : SaveData<WorkShopRequestForm>
{
	public string requestingCharacterID;

	public EQUIPMENT_TYPE equipmentType;

	public bool isSubjectForRemoval;

	public override void Save(WorkShopRequestForm data)
	{
		base.Save(data);
		requestingCharacterID = data.requestingCharacter.persistentID;
		equipmentType = data.equipmentType;
		isSubjectForRemoval = data.isSubjectForRemoval;
	}

	public override WorkShopRequestForm Load()
	{
		return new WorkShopRequestForm(this);
	}
}
