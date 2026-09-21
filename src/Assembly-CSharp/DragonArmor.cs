public class DragonArmor : ArmorItem
{
	public DragonArmor()
	{
		Initialize(TILE_OBJECT_TYPE.DRAGON_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public DragonArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
