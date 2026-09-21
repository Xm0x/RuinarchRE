namespace Goap.Unique_Action_Data;

public class SaveDataCraftEquipmentUAD : SaveDataUniqueActionData
{
	public CONCRETE_RESOURCES resourceUsedForCrafting;

	public override void Save(UniqueActionData data)
	{
		base.Save(data);
		CraftEquipmentUAD craftEquipmentUAD = data as CraftEquipmentUAD;
		resourceUsedForCrafting = craftEquipmentUAD.resourceUsedForCrafting;
	}

	public override UniqueActionData Load()
	{
		return new CraftEquipmentUAD(this);
	}
}
