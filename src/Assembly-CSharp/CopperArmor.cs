public class CopperArmor : ArmorItem
{
	public CopperArmor()
	{
		Initialize(TILE_OBJECT_TYPE.COPPER_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public CopperArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
