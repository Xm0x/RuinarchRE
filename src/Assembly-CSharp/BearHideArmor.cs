public class BearHideArmor : ArmorItem
{
	public BearHideArmor()
	{
		Initialize(TILE_OBJECT_TYPE.BEAR_HIDE_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public BearHideArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
