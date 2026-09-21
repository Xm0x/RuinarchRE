public class IronArmor : ArmorItem
{
	public IronArmor()
	{
		Initialize(TILE_OBJECT_TYPE.IRON_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public IronArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
