public class WorkShopRequestForm
{
	public Character requestingCharacter;

	public EQUIPMENT_TYPE equipmentType;

	public bool isSubjectForRemoval;

	public WorkShopRequestForm()
	{
	}

	public WorkShopRequestForm(SaveDataWorkShopRequestForm p_data)
	{
		equipmentType = p_data.equipmentType;
		isSubjectForRemoval = p_data.isSubjectForRemoval;
	}

	public override string ToString()
	{
		return requestingCharacter.name + " - " + equipmentType.ToStringEnum();
	}
}
