namespace Goap.Unique_Action_Data;

public class CraftEquipmentUAD : UniqueActionData
{
	public CONCRETE_RESOURCES resourceUsedForCrafting;

	public CraftEquipmentUAD()
	{
		resourceUsedForCrafting = CONCRETE_RESOURCES.Wood;
	}

	public CraftEquipmentUAD(SaveDataCraftEquipmentUAD saveData)
	{
		resourceUsedForCrafting = saveData.resourceUsedForCrafting;
	}

	public override SaveDataUniqueActionData Save()
	{
		SaveDataCraftEquipmentUAD saveDataCraftEquipmentUAD = new SaveDataCraftEquipmentUAD();
		saveDataCraftEquipmentUAD.Save(this);
		return saveDataCraftEquipmentUAD;
	}

	public void SetResourceUsedForCrafting(CONCRETE_RESOURCES p_resources)
	{
		resourceUsedForCrafting = p_resources;
	}
}
